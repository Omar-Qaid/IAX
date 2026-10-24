using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

/// <summary>
/// Sales invoice posting (CycleSalesorder.md Phase 8 / ARTables.md §8). Invoices delivered,
/// uninvoiced quantities only (packing-slip-based policy) and supports partial invoicing.
/// One voucher posts, from configuration:
///   Dr Customer summary (CustLedgerAccounts)                net + tax
///   Cr Sales revenue  (InventPosting SalesRevenue)          net
///   Cr Sales tax payable (TaxLedgerAccountGroup)            tax
///   Dr Cost of units invoiced (InventPosting SalesConsump)  COGS
///   Cr Inventory issue        (InventPosting SalesIssue)    COGS
/// and writes CustInvoiceJour/Trans, CustTrans + CustTransOpen, TaxTrans, Deducted -> Sold
/// inventory portions, and the line's financial remainders. COGS is the provisional running
/// average; the FIFO inventory close settles it to actual cost.
/// </summary>
public sealed class SalesInvoiceService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
    ISysNumberSequenceService numbers) : ISalesInvoiceService
{
    private DbContext Db => unitOfWork.Context;

    public Task<SalesInvoiceResult> PostAsync(long salesRecId, SalesInvoicePostDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(salesRecId, input, ct));

    private async Task<SalesInvoiceResult> PostCoreAsync(long salesRecId, SalesInvoicePostDto input, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var date = (input.InvoiceDate ?? DateTime.UtcNow).Date;
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = await Db.Set<SalesTable>().FirstOrDefaultAsync(x => x.RecId == salesRecId && x.DataAreaId == area, ct);
            if (order == null) return await Fail(404, "Sales order was not found in the selected company.", ct);
            if (order.SalesType == SalesType.ReturnItem)
                return await Fail(422, "Return orders are credited through the return order credit note.", ct);
            await support.LockAsync($"SalesInvoice:{area}:{order.SalesId}", ct);

            if (!string.IsNullOrWhiteSpace(input.RequestId))
            {
                var previous = await Db.Set<CustInvoiceJour>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area
                    && x.SalesId == order.SalesId && x.ParmId == input.RequestId, ct);
                if (previous != null)
                {
                    await unitOfWork.CommitTransactionAsync(ct);
                    return new(200, Summary(previous), "Sales invoice already posted for this request.");
                }
            }

            var invoiceAccount = string.IsNullOrWhiteSpace(order.InvoiceAccount) ? order.CustAccount : order.InvoiceAccount;
            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == invoiceAccount, ct);
            if (customer == null) return await Fail(422, "The invoice customer was not found.", ct);
            if (customer.Blocked is CustVendorBlocked.All or CustVendorBlocked.Invoicing)
                return await Fail(422, "The invoice customer is blocked for invoicing.", ct);

            var lines = await Db.Set<SalesLine>().Where(x => x.DataAreaId == area && x.SalesId == order.SalesId).ToListAsync(ct);
            var selection = new List<(SalesLine Line, decimal Qty)>();
            if (input.Lines == null || input.Lines.Count == 0)
            {
                selection.AddRange(lines.Where(x => x.RemainSalesFinancial - x.RemainSalesPhysical > 0)
                    .Select(x => (x, x.RemainSalesFinancial - x.RemainSalesPhysical)));
            }
            else
            {
                foreach (var request in input.Lines)
                {
                    var line = lines.FirstOrDefault(x => x.RecId == request.SalesLineRecId);
                    if (line == null) return await Fail(404, $"Sales line {request.SalesLineRecId} was not found on this order.", ct);
                    if (request.Qty <= 0) return await Fail(422, "The invoice quantity must be greater than zero.", ct);
                    var deliveredNotInvoiced = line.RemainSalesFinancial - line.RemainSalesPhysical;
                    if (request.Qty > deliveredNotInvoiced)
                        return await Fail(422, $"Invoice quantity for item {line.ItemId} exceeds the delivered, uninvoiced quantity {deliveredNotInvoiced}.", ct);
                    selection.Add((line, request.Qty));
                }
            }
            if (selection.Count == 0) return await Fail(422, "There is no delivered, uninvoiced quantity to invoice.", ct);

            var ledger = await support.LoadLedgerContextAsync(area, date, ct);
            var customerAccount = await support.ResolveCustomerSummaryAsync(area, order.PostingProfile, customer, ct);
            var invoiceId = (await numbers.NextAsync("CustInvoiceJour", area, ct)).Code;
            var entry = support.NewEntry(area, invoiceId, invoiceId, date, ledger);
            Db.Add(entry);
            await unitOfWork.CompleteAsync(ct);
            var currency = ledger.Ledger.AccountingCurrency;

            decimal totalRevenue = 0, totalTax = 0, totalQty = 0;
            var taxByCode = new Dictionary<string, (decimal Base, decimal Tax, string Group, string ItemGroup)>();
            foreach (var (line, qty) in selection)
            {
                var inventQty = qty == line.RemainSalesFinancial
                    ? line.RemainInventFinancial
                    : decimal.Round(qty * line.QtyOrdered / (line.SalesQty == 0 ? 1 : line.SalesQty), 6);
                var ratio = line.SalesQty == 0 ? 0m : qty / line.SalesQty;
                var net = decimal.Round((line.LineAmount - line.LineDisc - line.MultiLnDisc) * ratio, 2, MidpointRounding.AwayFromZero);

                var codes = await support.ApplicableTaxCodesAsync(area, line.TaxGroup, line.TaxItemGroup, date, ct);
                var rate = codes.Sum(x => x.Rate);
                var lineTax = order.InclTax && rate > 0
                    ? decimal.Round(net * rate / (100m + rate), 2, MidpointRounding.AwayFromZero)
                    : decimal.Round(net * rate / 100m, 2, MidpointRounding.AwayFromZero);
                var revenue = order.InclTax ? net - lineTax : net;
                foreach (var (code, codeTax) in SubledgerPostingSupport.AllocateTax(codes, lineTax))
                {
                    var current = taxByCode.GetValueOrDefault(code);
                    taxByCode[code] = (current.Base + revenue, current.Tax + codeTax, line.TaxGroup, line.TaxItemGroup);
                }

                var cogs = await IssueInventoryAsync(area, order, line, inventQty, invoiceId, date, ct);
                var revenueAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesRevenue, ct);
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, revenueAccount, LedgerPostingType.SalesRevenue, -revenue,
                    $"Sales {order.SalesId} item {line.ItemId}", -inventQty, currency));
                if (cogs != 0m)
                {
                    var consumption = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesConsump, ct);
                    var issueAccount = await support.ResolveInventPostingAsync(area, line.ItemId, InventAccountType.SalesIssue, ct);
                    Db.AddRange(
                        SubledgerPostingSupport.AccountEntry(area, entry.RecId, consumption, LedgerPostingType.SalesConsump, cogs,
                            $"Cost of {line.ItemId} invoiced on {invoiceId}", inventQty, currency),
                        SubledgerPostingSupport.AccountEntry(area, entry.RecId, issueAccount, LedgerPostingType.SalesIssue, -cogs,
                            $"Inventory issue {line.ItemId} invoiced on {invoiceId}", -inventQty, currency));
                }

                Db.Set<CustInvoiceTrans>().Add(new CustInvoiceTrans
                {
                    DataAreaId = area, InvoiceId = invoiceId, InvoiceDate = date, SalesId = order.SalesId,
                    LineNum = line.LineNum, ItemId = line.ItemId, InventDimId = line.InventDimId,
                    InventTransId = line.InventTransId, Qty = qty, QtyPhysical = qty, InventQty = inventQty,
                    SalesPrice = line.SalesPrice, PriceUnit = line.PriceUnit, SalesUnit = line.SalesUnit,
                    LineAmount = revenue, LineAmountMst = revenue, TaxAmount = lineTax,
                    TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup, CurrencyCode = order.CurrencyCode
                });

                line.RemainSalesFinancial -= qty;
                line.RemainInventFinancial = Math.Max(0m, line.RemainInventFinancial - inventQty);
                if (line.RemainSalesFinancial <= 0 && line.RemainSalesPhysical <= 0) line.SalesStatus = SalesStatus.Invoiced;
                totalRevenue += revenue;
                totalTax += lineTax;
                totalQty += qty;
            }

            foreach (var (code, value) in taxByCode)
            {
                if (value.Tax == 0m) continue;
                var payable = await support.ResolveTaxPayableAsync(area, code, ct);
                Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, payable, LedgerPostingType.Tax, -value.Tax,
                    $"Sales tax {code} on {invoiceId}", 0m, currency));
                Db.Set<TaxTrans>().Add(TaxTransaction(area, date, invoiceId, code, value.Group, value.ItemGroup,
                    order.CurrencyCode, currency, -value.Base, -value.Tax));
            }

            var total = totalRevenue + totalTax;
            Db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, customerAccount, LedgerPostingType.CustBalance, total,
                $"Customer invoice {invoiceId}", 0m, currency));
            Db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = invoiceId, VoucherDataAreaId = area, AccountingDate = date
            });
            await AddCustomerTransactionAsync(area, order, invoiceAccount, invoiceId, date, total, LedgerTransType.Sales,
                $"Sales order {order.SalesId}", ct);

            var journal = new CustInvoiceJour
            {
                DataAreaId = area, InvoiceId = invoiceId, InvoiceDate = date, SalesId = order.SalesId,
                OrderAccount = order.CustAccount, InvoiceAccount = invoiceAccount, CustGroup = order.CustGroup,
                CurrencyCode = order.CurrencyCode, LedgerVoucher = invoiceId, ParmId = input.RequestId ?? string.Empty,
                Qty = totalQty, SalesBalance = totalRevenue, SumTax = totalTax, InvoiceAmount = total,
                SalesBalanceMst = totalRevenue, SumTaxMst = totalTax, InvoiceAmountMst = total,
                Payment = order.PaymTerm, PostingProfile = order.PostingProfile, DueDate = date
            };
            Db.Set<CustInvoiceJour>().Add(journal);

            if (lines.All(x => x.SalesStatus == SalesStatus.Invoiced || x.SalesStatus == SalesStatus.Canceled))
                order.SalesStatus = SalesStatus.Invoiced;
            order.DocumentStatus = DocumentStatus.Invoice;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, Summary(journal), "Sales invoice posted.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // Financial issue: Deducted -> Sold for the invoiced inventory quantity at provisional running-average cost.
    private async Task<decimal> IssueInventoryAsync(string area, SalesTable order, SalesLine line, decimal inventQty,
        string invoiceId, DateTime date, CancellationToken ct)
    {
        var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(x => x.DataAreaId == area
            && x.InventTransId == line.InventTransId && x.ReferenceId == order.SalesId
            && x.ReferenceCategory == InventRefType.SalesTable, ct)
            ?? throw new SubledgerPostingException($"Inventory origin is missing for item {line.ItemId}.");
        var deducted = (await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
            && x.StatusIssue == StatusIssue.Deducted).ToListAsync(ct)).OrderBy(x => x.RecId).ToList();
        if (deducted.Sum(x => -x.Qty) < inventQty)
            throw new SubledgerPostingException($"Delivered inventory does not cover the invoice for item {line.ItemId}.");

        var site = await Db.Set<InventDim>().AsNoTracking().Where(x => x.DataAreaId == area && x.InventDimId == line.InventDimId)
            .Select(x => x.InventSiteId).FirstOrDefaultAsync(ct) ?? string.Empty;
        var unitCost = await ReceiptRunningAverageAsync(area, line.ItemId, site, ct);
        var left = inventQty;
        foreach (var source in deducted)
        {
            if (left <= 0) break;
            var available = -source.Qty;
            var take = Math.Min(available, left);
            if (take < available)
                Db.Set<InventTrans>().Add(new InventTrans
                {
                    DataAreaId = source.DataAreaId, InventTransOrigin = source.InventTransOrigin, ItemId = source.ItemId,
                    InventDimId = source.InventDimId, Qty = -(available - take), StatusIssue = StatusIssue.Deducted,
                    StatusReceipt = StatusReceipt.None, ValueOpen = InventTransOpen.Yes, CurrencyCode = source.CurrencyCode,
                    DateExpected = source.DateExpected, DatePhysical = source.DatePhysical, DateStatus = source.DateStatus,
                    DateInvent = source.DateInvent, PackingSlipId = source.PackingSlipId, VoucherPhysical = source.VoucherPhysical
                });
            source.Qty = -take;
            source.StatusIssue = StatusIssue.Sold;
            source.DateFinancial = date;
            source.DateStatus = date;
            source.InvoiceId = invoiceId;
            source.Voucher = invoiceId;
            source.CostAmountPosted = -decimal.Round(take * unitCost, 2, MidpointRounding.AwayFromZero);
            left -= take;
        }
        var cogs = decimal.Round(inventQty * unitCost, 2, MidpointRounding.AwayFromZero);
        var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct)
            ?? throw new SubledgerPostingException($"Inventory summary is missing for item {line.ItemId}.");
        summary.Deducted -= inventQty;   // no longer delivered-not-invoiced
        summary.PostedQty -= inventQty;  // financially issued (physical already reduced at packing slip)
        summary.PostedValue -= cogs;
        summary.LastUpdDatePhysical = date;
        return cogs;
    }

    // Provisional issue cost = weighted average of financially posted receipts in the item/site pool.
    private async Task<decimal> ReceiptRunningAverageAsync(string area, string itemId, string site, CancellationToken ct)
    {
        var dimIds = await Db.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.InventSiteId == site).Select(x => x.InventDimId).ToListAsync(ct);
        var receipts = await Db.Set<InventTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ItemId == itemId && dimIds.Contains(x.InventDimId)
                && x.StatusReceipt == StatusReceipt.Purchased && x.Qty > 0)
            .Select(x => new { x.Qty, x.CostAmountPosted, x.CostAmountAdjustment }).ToListAsync(ct);
        var qty = receipts.Sum(x => x.Qty);
        return qty <= 0 ? 0m : receipts.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) / qty;
    }

    private async Task AddCustomerTransactionAsync(string area, SalesTable order, string invoiceAccount, string invoiceId,
        DateTime date, decimal amount, LedgerTransType type, string text, CancellationToken ct)
    {
        var custTrans = new CustTrans
        {
            DataAreaId = area, AccountNum = invoiceAccount, OrderAccount = order.CustAccount, TransDate = date,
            Voucher = invoiceId, Invoice = invoiceId, DocumentNum = invoiceId, DocumentDate = date, Txt = text,
            TransType = type, CurrencyCode = order.CurrencyCode, AmountCur = amount, AmountMst = amount,
            ReportingCurrencyAmount = amount, DueDate = date, PaymTermId = order.PaymTerm, PaymMode = order.PaymMode,
            PostingProfile = order.PostingProfile, DefaultDimension = order.DefaultDimension
        };
        Db.Set<CustTrans>().Add(custTrans);
        await unitOfWork.CompleteAsync(ct);
        Db.Set<CustTransOpen>().Add(new CustTransOpen
        {
            DataAreaId = area, AccountNum = invoiceAccount, RefRecId = custTrans.RecId, TransDate = date,
            DueDate = date, AmountCur = amount, AmountMst = amount, ReportingCurrencyAmount = amount
        });
    }

    internal static TaxTrans TaxTransaction(string area, DateTime date, string voucher, string code, string group,
        string itemGroup, string sourceCurrency, string currency, decimal baseAmount, decimal taxAmount) => new()
    {
        DataAreaId = area, TransDate = date, Voucher = voucher, TaxCode = code, TaxGroup = group, TaxItemGroup = itemGroup,
        TaxDirection = TaxDirection.Outgoing, SourceCurrencyCode = sourceCurrency, CurrencyCode = currency,
        SourceBaseAmountCur = baseAmount, SourceTaxAmountCur = taxAmount, TaxBaseAmountCur = baseAmount, TaxAmountCur = taxAmount,
        TaxBaseAmount = baseAmount, TaxAmount = taxAmount, TaxBaseAmountRep = baseAmount, TaxAmountRep = taxAmount
    };

    private static object Summary(CustInvoiceJour journal) => new
    {
        invoiceId = journal.InvoiceId, salesId = journal.SalesId, invoiceDate = journal.InvoiceDate,
        qty = journal.Qty, salesBalance = journal.SalesBalance, sumTax = journal.SumTax, invoiceAmount = journal.InvoiceAmount
    };

    private async Task<SalesInvoiceResult> Fail(int status, string message, CancellationToken ct)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
