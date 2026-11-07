using System.Data;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

public sealed partial class PackingSlipJournalService
{
    public Task<PackingSlipJournalResult> CancelAsync(long salesOrderRecId, long packingSlipRecId,
        DateTime? accountingDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var area = _company.GetDataAreaId();
            var changed = false;
            try
            {
                await LockPostingKeyAsync($"PackingSlip:{area}:{salesOrderRecId}", ct);
                var order = await Db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == salesOrderRecId, ct);
                if (order is null) return new PackingSlipJournalResult(404, null, "Sales order was not found.");
                var support = new SubledgerPostingSupport(Db);
                await support.LockAsync($"SalesInvoice:{area}:{order.SalesId}", ct);
                var slip = await Db.Set<CustPackingSlipJour>().SingleOrDefaultAsync(x =>
                    x.DataAreaId == area && x.SalesId == order.SalesId && x.RecId == packingSlipRecId, ct);
                if (slip is null) return new PackingSlipJournalResult(404, null, "Packing slip was not found.");
                if (order.SalesType == SalesType.ReturnItem || order.SalesStatus == SalesStatus.Canceled)
                    return new PackingSlipJournalResult(422, null, "Only sales deliveries on active orders can be canceled.");
                if (slip.PostedState == NoYes.No && slip.Qty == 0 && slip.InternalPackingSlipId == slip.PackingSlipId + ".2")
                    return new PackingSlipJournalResult(200, ToSummary(slip), "Packing slip canceled.");
                var date = (accountingDate ?? DateTime.UtcNow).Date;
                if (slip.PostedState != NoYes.Yes || slip.Qty <= 0 || date < slip.DeliveryDate.Date)
                    return new PackingSlipJournalResult(422, null, "A posted delivery and an accounting date on or after delivery are required.");
                var cancellationVersion = slip.PackingSlipId + ".2";
                if (cancellationVersion.Length > FieldLengths.PackingSlipId)
                    return new PackingSlipJournalResult(422, null, "The packing slip number is too long for its version identifier.");

                var lines = await Db.Set<CustPackingSlipTrans>().Where(x => x.DataAreaId == area
                    && x.ParentRecId == slip.RecId).ToListAsync(ct);
                if (lines.Count == 0 || lines.Sum(x => x.Qty) != slip.Qty || lines.Any(x =>
                    x.Qty <= 0 || x.InventQty <= 0 || x.PdsCwQty != 0 || x.FullyMatched == NoYes.Yes))
                    return new PackingSlipJournalResult(422, null, "The delivery lines cannot be fully canceled.");
                foreach (var key in lines.Select(x => $"InventStock:{area}:{x.ItemId}:{x.InventDimId}")
                    .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
                    await LockPostingKeyAsync(key, ct);

                var context = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Customer, _currentUser.GetCurrentUserId(), ct);
                var voucher = string.Empty;
                GeneralJournalEntry? reversal = null;
                var now = DateTime.UtcNow;
                changed = true;
                foreach (var line in lines)
                {
                    var salesLine = await Db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == area
                        && x.SalesId == order.SalesId && x.InventTransId == line.InventTransId, ct);
                    var origin = await Db.Set<InventTransOrigin>().SingleOrDefaultAsync(x => x.DataAreaId == area
                        && x.InventTransId == line.InventTransId && x.ReferenceId == order.SalesId, ct);
                    if (salesLine is null || origin is null || salesLine.SalesStatus == SalesStatus.Canceled
                        || salesLine.RemainSalesPhysical + line.Qty > salesLine.RemainSalesFinancial)
                        return new PackingSlipJournalResult(422, null, "The delivery is invoiced or its order line has changed.");
                    var issues = await Db.Set<InventTrans>().Where(x => x.DataAreaId == area
                        && x.InventTransOrigin == origin.RecId && x.PackingSlipId == slip.PackingSlipId).ToListAsync(ct);
                    var stock = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
                        && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct);
                    if (stock is null || stock.Deducted < line.InventQty || issues.Sum(x => -x.Qty) != line.InventQty
                        || issues.Any(x => x.StatusIssue != StatusIssue.Deducted || x.ItemId != line.ItemId
                            || x.InventDimId != line.InventDimId || x.PdscwQty != 0 || x.Qty >= 0))
                        return new PackingSlipJournalResult(422, null, "The delivery's outstanding inventory issues could not be reconciled.");

                    var cost = issues.Sum(x => -x.CostAmountPhysical);
                    var physicalPosting = await Db.Set<InventTransPosting>().AnyAsync(x => x.DataAreaId == area
                        && x.InventTransOrigin == origin.RecId && x.InventTransPostingType == InventTransPostingType.Physical
                        && x.Voucher == slip.PackingSlipId && x.IsPosted == NoYes.Yes, ct);
                    if (physicalPosting)
                    {
                        if (cost == 0 || (!string.IsNullOrEmpty(context.Ledger.ReportingCurrency)
                            && context.Ledger.ReportingCurrency != context.Ledger.AccountingCurrency))
                            return new PackingSlipJournalResult(422, null, "The original physical posting cannot be reversed in the ledger currencies.");
                        if (reversal is null)
                        {
                            voucher = await NextPackingSlipIdAsync(area, ct);
                            reversal = support.NewEntry(area, voucher, slip.PackingSlipId, date, context);
                            Db.Add(reversal);
                            await Db.SaveChangesAsync(ct);
                            Db.Add(new SubledgerVoucherGeneralJournalEntry
                            {
                                DataAreaId = area, VoucherDataAreaId = area, Voucher = voucher,
                                GeneralJournalEntry = reversal.RecId, AccountingDate = date
                            });
                        }
                        await new PackingSlipPhysicalPosting(Db).ReverseAsync(area, origin.RecId, reversal.RecId,
                            date, context.Ledger.AccountingCurrency, cost, ct, slip.PackingSlipId);
                    }
                    else if (issues.Any(x => !string.IsNullOrEmpty(x.VoucherPhysical)))
                        return new PackingSlipJournalResult(422, null, "The original physical posting is missing.");

                    foreach (var issue in issues)
                    {
                        issue.StatusIssue = StatusIssue.Ordered;
                        issue.DateStatus = date;
                        issue.DatePhysical = issue.DateInvent = default;
                        issue.PackingSlipId = issue.VoucherPhysical = string.Empty;
                        issue.CostAmountPhysical = 0;
                    }
                    stock.Deducted -= line.InventQty;
                    stock.OnOrder += line.InventQty;
                    stock.PhysicalInvent += line.InventQty;
                    stock.PhysicalValue += cost;
                    stock.AvailPhysical = stock.PhysicalInvent - stock.ReservPhysical - stock.Picked;
                    stock.LastUpdDatePhysical = date;
                    salesLine.RemainSalesPhysical += line.Qty;
                    salesLine.RemainInventPhysical += line.InventQty;
                    salesLine.SalesStatus = SalesStatus.Backorder;
                    var history = await Db.Set<CustPackingSlipTransHistory>().SingleOrDefaultAsync(x =>
                        x.DataAreaId == area && x.CustPackingSlipTrans == line.RecId && x.ValidTo == OpenHistoryEnd, ct);
                    if (history is not null)
                        history.ValidTo = now;
                }

                slip.Qty = 0;
                slip.PostedState = NoYes.No;
                slip.InternalPackingSlipId = cancellationVersion;
                slip.LedgerVoucher = voucher;
                order.SalesStatus = SalesStatus.Backorder;
                Db.Add(new CustPackingSlipVersion
                {
                    CustPackingSlipJour = slip.RecId, InternalPackingSlipId = cancellationVersion,
                    LedgerVoucher = voucher, AccountingDate = date, VersionDateTime = now, ParmId = slip.ParmId
                });
                await _unitOfWork.CompleteAsync(ct);
                await transaction.CommitAsync(ct);
                changed = false;
                return new PackingSlipJournalResult(200, ToSummary(slip), "Packing slip canceled.");
            }
            catch (SubledgerPostingException exception)
            {
                return new PackingSlipJournalResult(422, null, exception.Message);
            }
            finally
            {
                if (changed) Db.ChangeTracker.Clear();
            }
        });
}
