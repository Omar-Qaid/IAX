using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed partial class ProductReceiptService
{
    private static readonly DateTime OpenReceiptHistory = new(2154, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    public Task<PurchResult> CorrectAsync(long recId, ProductReceiptCorrectionDto input, CancellationToken ct) =>
        input.ReceiptLineRecId <= 0 || input.Qty <= 0 || decimal.Round(input.Qty, 4) != input.Qty
            ? Task.FromResult(new PurchResult(400, null, "A receipt line and positive corrected quantity are required."))
            : Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CorrectCoreAsync(recId, input.ReceiptLineRecId, input.Qty, input.AccountingDate.Date, ct));

    public Task<PurchResult> CancelAsync(long recId, DateTime accountingDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CorrectCoreAsync(recId, null, 0m, accountingDate.Date, ct));

    private async Task<PurchResult> CorrectCoreAsync(long recId, long? lineId, decimal quantity, DateTime date, CancellationToken ct)
    {
        if (date == default) return new(400, null, "An accounting date is required.");
        var cancel = lineId == null;

        var area = _company.GetDataAreaId();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var purchaseId = await Db.Set<VendPackingSlipJour>().Where(x => x.DataAreaId == area && x.RecId == recId)
                .Select(x => x.PurchId).SingleOrDefaultAsync(ct);
            if (purchaseId == null) return await Fail(404, "Product receipt was not found.", ct);
            var support = new SubledgerPostingSupport(Db);
            await support.LockAsync($"PurchDocument:{area}:{purchaseId}", ct);
            var receipt = await Db.Set<VendPackingSlipJour>().SingleAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            var sources = await Db.Set<VendPackingSlipTrans>().Where(x => x.DataAreaId == area
                && x.VendPackingSlipJour == recId && (lineId == null || x.RecId == lineId)).OrderBy(x => x.RecId).ToListAsync(ct);
            if (sources.Count == 0) return await Fail(404, "Product receipt line was not found.", ct);
            if (cancel && (sources.Any(x => x.Qty < 0) || sources.Sum(x => x.Qty) != receipt.Qty))
                return await Fail(422, "Only a purchase receipt with consistent quantities can be canceled.", ct);

            var lastVersion = await Db.Set<VendPackingSlipVersion>().Where(x => x.VendPackingSlipJour == recId)
                .OrderByDescending(x => x.VersionDateTime).FirstOrDefaultAsync(ct);
            if (sources.All(x => x.Qty == quantity))
            {
                await _unitOfWork.CommitTransactionAsync(ct);
                return new(200, receipt, "Product receipt already has the requested quantity.");
            }
            if (sources.Any(x => quantity > x.Qty) || date < (lastVersion?.AccountingDate ?? receipt.DeliveryDate).Date)
                return await Fail(422, "Correction must reduce the quantity and cannot precede the last receipt posting date.", ct);
            var sourceIds = sources.Select(x => x.RecId).ToArray();
            if (sources.Any(x => x.FullyMatched == NoYes.Yes) || await Db.Set<VendInvoicePackingSlipQuantityMatch>().AnyAsync(
                x => x.DataAreaId == area && sourceIds.Contains(x.PackingSlipTransRecId), ct))
                return await Fail(422, "An invoiced receipt line cannot be corrected.", ct);

            var order = await Db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.PurchId == purchaseId, ct);
            if (order == null || order.PurchStatus == PurchStatus.Canceled)
                return await Fail(422, "The receipt requires an active purchase order.", ct);
            var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, _currentUser.GetCurrentUserId(), ct);
            var voucher = (await _numbers.NextAsync("VendPackingSlipJour", area, ct)).Code;
            GeneralJournalEntry? entry = null;
            var originalTime = lastVersion?.VersionDateTime ?? receipt.CreatedAt ?? receipt.DeliveryDate;
            var changedAt = DateTime.UtcNow;
            if (changedAt <= originalTime) changedAt = originalTime.AddTicks(1);
            if (lastVersion == null)
                Db.Add(ReceiptVersion(receipt, receipt.InternalPackingSlipId, receipt.LedgerVoucher, receipt.DeliveryDate, originalTime));

            foreach (var source in sources.Where(x => x.Qty != quantity))
            {
                var line = await Db.Set<PurchLine>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.PurchId == purchaseId
                    && x.LineNumber == source.LineNum && x.InventTransId == source.InventTransId, ct);
                if (line == null || line.PurchStatus == PurchStatus.Canceled)
                    return await Fail(422, "The receipt requires an active purchase order line.", ct);

                var removed = source.Qty - quantity;
                var newInventQty = decimal.Round(source.InventQty * quantity / source.Qty, 4, MidpointRounding.AwayFromZero);
                var removedInvent = source.InventQty - newInventQty;
                var amount = source.ValueMst - decimal.Round(source.ValueMst * quantity / source.Qty, 4, MidpointRounding.AwayFromZero);
                var accrual = await Db.Set<PurchReceiptAccrual>().SingleOrDefaultAsync(x => x.DataAreaId == area
                    && x.PackingSlipTransRecId == source.RecId, ct);
                if (accrual != null)
                {
                    if (entry == null)
                    {
                        entry = support.NewEntry(area, voucher, voucher, date, ledger);
                        Db.Add(entry);
                        await _unitOfWork.CompleteAsync(ct);
                        Db.Add(new SubledgerVoucherGeneralJournalEntry
                        {
                            DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date
                        });
                    }
                    var reversal = new VendInvoicePackingSlipQuantityMatch
                    {
                        PackingSlipTransRecId = source.RecId,
                        InventQuantity = removedInvent
                    };
                    await new PurchaseReceiptAccrualPosting(Db).ReverseAsync(area, reversal, entry.RecId, ledger.Ledger.AccountingCurrency, ct);
                    amount = reversal.AccrualReversalAmountMst;
                    if (source.SourceDocumentLine is long sourceLineId && receipt.SourceDocumentHeader is long headerId)
                    {
                        var accounting = new PurchaseSourceAccounting(Db);
                        await accounting.CaptureAsync(area, entry, headerId, sourceLineId, ct);
                        await accounting.VerifyBalancedAsync(entry, ct);
                    }
                }

                if (source.InventQty > 0)
                {
                    if ((!cancel && newInventQty <= 0) || removedInvent <= 0)
                        return await Fail(422, "Correction must leave a positive inventory quantity and reduce it at the supported precision.", ct);
                    var error = await RestoreCorrectedSupplyAsync(source, newInventQty, amount, date, ct);
                    if (error != null) return await Fail(422, error, ct);
                }

                var history = await Db.Set<VendPackingSlipTransHistory>().SingleOrDefaultAsync(x => x.DataAreaId == area
                    && x.VendPackingSlipTrans == source.RecId && x.ValidTo == OpenReceiptHistory, ct);
                if (history == null)
                    Db.Add(ReceiptHistory(source, originalTime, changedAt));
                else
                    history.ValidTo = changedAt;

                line.RemainPurchPhysical += removed;
                line.RemainInventPhysical += removedInvent;
                line.PurchStatus = PurchStatus.Backorder;
                if (order.PurchStatus == PurchStatus.Received) order.PurchStatus = PurchStatus.Backorder;
                source.Qty = quantity;
                source.InventQty = newInventQty;
                source.Remain = line.RemainPurchPhysical;
                source.RemainInvent = line.RemainInventPhysical;
                source.ValueMst -= amount;
                receipt.Qty -= removed;
                Db.Add(ReceiptHistory(source, changedAt, OpenReceiptHistory));
            }
            Db.Add(ReceiptVersion(receipt, voucher, entry == null ? string.Empty : voucher, date, changedAt));
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, receipt, cancel ? "Product receipt canceled." : "Product receipt corrected.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<string?> RestoreCorrectedSupplyAsync(VendPackingSlipTrans source, decimal newQty,
        decimal amount, DateTime date, CancellationToken ct)
    {
        var area = source.DataAreaId;
        var error = await InventoryFinancialBoundary.CheckAsync(Db, area, source.ItemId, source.DeliveryDate, ct)
            ?? await InventoryCountProtection.CheckAsync(Db, area, source.ItemId, source.InventDimId, ct);
        if (error != null) return error;

        var origin = await Db.Set<InventTransOrigin>().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.InventTransId == source.InventTransId && x.ReferenceCategory == InventRefType.PurchaseOrder
            && x.ReferenceId == source.OrigPurchId, ct);
        if (origin == null) return "The original purchase inventory transaction could not be found.";
        var rows = await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
            && x.InventDimId == source.InventDimId && x.PackingSlipId == source.InternalPackingSlipId
            && x.StatusReceipt == StatusReceipt.Received).OrderBy(x => x.RecId).ToListAsync(ct);
        if (rows.Any(x => x.Qty <= 0) || rows.Sum(x => x.Qty) != source.InventQty
            || rows.Sum(x => x.CostAmountPhysical) != source.ValueMst)
            return "The original receipt inventory is no longer available for correction.";
        var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.ItemId == source.ItemId && x.InventDimId == source.InventDimId, ct);
        var removed = source.InventQty - newQty;
        if (summary == null || summary.Received < removed || summary.PhysicalInvent - summary.ReservPhysical - summary.Picked < removed)
            return "There is insufficient available physical inventory for the correction.";

        var keptQty = 0m;
        var keptCost = 0m;
        var newCost = source.ValueMst - amount;
        foreach (var row in rows)
        {
            var keep = Math.Min(row.Qty, newQty - keptQty);
            if (keep == 0)
            {
                row.StatusReceipt = StatusReceipt.Ordered;
                row.DatePhysical = default;
                row.DateStatus = default;
                row.DateInvent = default;
                row.PackingSlipId = string.Empty;
                row.VoucherPhysical = string.Empty;
                row.CostAmountPhysical = 0m;
                continue;
            }
            if (keep < row.Qty) Db.Add(CopyOrdered(row, row.Qty - keep));
            row.Qty = keep;
            keptQty += keep;
            var totalCost = decimal.Round(newCost * keptQty / newQty, 4, MidpointRounding.AwayFromZero);
            row.CostAmountPhysical = totalCost - keptCost;
            keptCost = totalCost;
        }
        summary.Ordered += removed;
        summary.Received -= removed;
        summary.PhysicalInvent -= removed;
        summary.PhysicalValue -= amount;
        summary.AvailPhysical = summary.PhysicalInvent - summary.ReservPhysical - summary.Picked;
        summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
        summary.LastUpdDatePhysical = date;
        return null;
    }

    private static VendPackingSlipTransHistory ReceiptHistory(VendPackingSlipTrans line, DateTime from, DateTime to) => new()
    {
        DataAreaId = line.DataAreaId, PackingSlipLine = line, Qty = line.Qty, InventQty = line.InventQty,
        Remain = line.Remain, RemainInvent = line.RemainInvent, ValueMst = line.ValueMst, ValidFrom = from, ValidTo = to
    };

    private static VendPackingSlipVersion ReceiptVersion(VendPackingSlipJour receipt, string number, string voucher, DateTime date, DateTime at) => new()
    {
        VendPackingSlipJour = receipt.RecId, InternalPackingSlipId = number, LedgerVoucher = voucher,
        AccountingDate = date, VersionDateTime = at, Qty = receipt.Qty
    };
}
