using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
public sealed class AccountsPayableTransactionFlow(ApplicationDbContext db, IPurchOrderService orders,
    IPurchConfirmationService confirmations, IProductReceiptService receipts, IVendInvoiceInfoService pending,
    IVendorInvoiceService invoices, IVendPaymentService payments, IVendSettleOpenTransactionsService settlements,
    IPurchReturnService returns, IVendSettlementReversalService undoSettlement, IVendPaymentReversalService undoPayment,
    AccountsPayableJournalExamples journals, IMarkupTransService charges)
{
    public static IReadOnlyDictionary<string, string> Scenarios { get; } = new Dictionary<string, string>
    {
        ["01"] = "Stock and service purchase to payment", ["02"] = "Partial receipts and invoices",
        ["03"] = "Non-stocked service purchase order", ["04"] = "Direct expense invoice journal", ["07"] = "Purchase delivery charge",
        ["11"] = "Payment-journal prepayment and application", ["12"] = "Cash discount", ["13"] = "Foreign-currency purchase and realized exchange difference",
        ["15"] = "Financial credit note without inventory", ["05"] = "Invoice across multiple receipts",
        ["06"] = "Price and quantity matching", ["08"] = "Partial payment", ["09"] = "On-account payment and later settlement",
        ["14"] = "Purchase return and vendor credit", ["16"] = "Cancel unreceived remainder",
        ["18"] = "Settlement reversal", ["19"] = "Payment reversal", ["20"] = "Validation failures and rollback"
    };
    private string Area => db.GetDataAreaId();
    private readonly List<AccountsPayableStageReport> steps = [];
    private static void Check(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException("AP cycle integrity: " + message);
    }
    private static void Success(PurchResult result, string stage)
    {
        if (result.StatusCode != 200) throw new InvalidOperationException($"AP {stage} failed ({result.StatusCode}): {result.Message}");
    }
    public async Task<AccountsPayableScenarioReport> ExecuteAsync(AccountsPayableTransactionOptions options, string id, CancellationToken ct)
    {
        steps.Clear();
        var key = $"APV2-{options.FiscalYear}-{id}";
        var date = new DateTime(options.FiscalYear, 4, 15);
        var account = $"APV2-{Area}-{(id == "13" ? "USD" : "LOCAL")}";
        var currency = id == "13" ? "USD" : "SAR";
        if (id is "04" or "15")
        {
            await journals.InvoiceAsync(key + "-DOC", account, key + "-I1", 402.50m, false, date, ct);
            if (id == "15")
            {
                await journals.InvoiceAsync(key + "-CREDIT", account, key + "-CR", 115m, true, date.AddDays(1), ct);
                await SettleMarked(key, account, date.AddDays(2), ct);
            }
            Success(await payments.PostAsync(new(account, "APV2-BANK", id == "15" ? 287.50m : 402.50m, key + "-PAY", date.AddDays(35), SettleInvoiceId: key + "-I1"), ct), "direct-invoice payment");
            steps.Add(new("Direct expense / financial credit", key, "Posted through invoice journals; no inventory movement"));
            return await Reconcile(key, id, 0m, 0m, 0m, ct);
        }
        if (id == "11") await journals.PrepaymentAsync(key + "-PRE", account, 1000m, date.AddDays(-13), ct);
        var serviceOnly = id == "03";
        var order = await db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.PurchName == key && x.PurchaseType != PurchaseType.ReturnItem, ct);
        if (order == null)
        {
            var result = await orders.CreateAsync(new(account, null, currency, date, "APV2-SITE", "APV2-WH", key, date.AddDays(-14)), ct);
            Success(result, "create order");
            order = (PurchTable)result.Data!;
        }
        Check(order.OrderAccount == account && order.CurrencyCode == currency, "existing example order differs.");
        await EnsureLine(order, serviceOnly ? "APV2-SVC" : "APV2-STOCK", serviceOnly ? 2m : 100m, serviceOnly ? 350m : id == "13" ? 7m : 25m, date, ct);
        if (id == "01") await EnsureLine(order, "APV2-SVC", 2m, 350m, date, ct);
        if (!await db.Set<VendPurchOrderJour>().AnyAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct))
            Success(await confirmations.ConfirmAsync(order.RecId, ct, date.AddDays(-12)), "confirm order");
        steps.Add(new("Purchase order and confirmation", order.PurchId, "Created/verified"));
        var lines = (await db.Set<PurchLine>().Where(x => x.DataAreaId == Area && x.PurchId == order.PurchId).ToListAsync(ct)).OrderBy(x => x.LineNumber).ToList();
        if (id == "07" && !await db.Set<MarkupTrans>().AnyAsync(x => x.DataAreaId == Area && x.TransRecId == order.RecId && x.TransTableId == FinanceTableNumbers.Unspecified, ct))
        {
            var result = await charges.CreateAsync("purchase", order.RecId, new MarkupTransDto { MarkupCode = "APV2-FRT",
                CurrencyCode = currency, Value = 50m, MarkupCategory = MarkupCategory.Fixed, TaxGroup = "APV2", TaxItemGroup = "APV2", TransDate = date }, cancellationToken: ct);
            if (result.Error != null) throw new InvalidOperationException("AP charge creation failed: " + result.Error.Message);
        }
        if (id == "02")
        {
            await Receipt(order, key + "-R1", date.AddDays(-10), [new(lines[0].RecId, 60m)], ct);
            await Invoice(order, key + "-I1", date.AddDays(-8), [new(lines[0].RecId, 40m)], ct);
            await Receipt(order, key + "-R2", date.AddDays(-5), [new(lines[0].RecId, 40m)], ct);
            await Invoice(order, key + "-I2", date, [new(lines[0].RecId, 60m)], ct);
        }
        else if (id == "05")
        {
            await Receipt(order, key + "-R1", date.AddDays(-10), [new(lines[0].RecId, 60m)], ct);
            await Receipt(order, key + "-R2", date.AddDays(-5), [new(lines[0].RecId, 40m)], ct);
            await Invoice(order, key + "-I1", date, [new(lines[0].RecId, 100m)], ct);
        }
        else
        {
            var quantities = lines.Select(x => new PurchPostLineDto(x.RecId, id is "16" or "20" ? 60m : x.PurchQty)).ToList();
            await Receipt(order, key + "-R1", date.AddDays(-5), quantities, ct);
            if (id == "16" && lines.Any(x => x.RemainPurchPhysical > 0))
            {
                Success(await orders.CancelRemainderAsync(order.RecId, ct), "cancel remainder");
                steps.Add(new("Cancel remainder", order.PurchId, "Unreceived quantity canceled; received quantity preserved"));
            }
            if (id is "06" or "20" && !await HasInvoice(key + "-I1", account, ct))
            {
                var before = await PostingCounts(ct);
                var result = await invoices.PostAsync(order.RecId, new(key + "-BAD", date, [new(lines[0].RecId, 1m, 30m)]), ct);
                Check(result.StatusCode == 422, "price mismatch was not rejected.");
                Check(before == await PostingCounts(ct), "rejected invoice left posting rows behind.");
                var over = await pending.CreateFromOrderAsync(order.RecId, new(key + "-OVER", date, date, Lines: [new(lines[0].RecId, 101m)]), ct);
                Check(over.StatusCode == 422 && before == await PostingCounts(ct), "excess quantity was not rejected atomically.");
                steps.Add(new("Negative validation", key, "Price mismatch and excess quantity rejected without posting"));
            }
            await Invoice(order, key + "-I1", date, quantities, ct);
        }
        var posted = await db.Set<VendInvoiceJour>().Where(x => x.DataAreaId == Area && x.InvoiceAccount == account && x.InvoiceId.StartsWith(key + "-I")).ToListAsync(ct);
        var total = posted.Sum(x => x.InvoiceAmount);
        decimal expectedPhysical = serviceOnly ? 0m : id is "16" or "20" ? 60m : 100m;
        decimal expectedOpenCredit = 0m, expectedOpenDebit = 0m;
        var paymentAmount = total;
        if (id == "11")
        {
            await SettleMarked(key, account, date.AddDays(1), ct);
            paymentAmount -= 1000m;
        }
        if (id == "12") paymentAmount = decimal.Round(total * 0.98m, 2, MidpointRounding.AwayFromZero);
        if (id == "14")
        {
            var original = await db.Set<VendInvoiceTrans>().SingleAsync(x => x.DataAreaId == Area && x.InvoiceId == key + "-I1" && x.ItemId == "APV2-STOCK", ct);
            var returnOrder = await db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.ReturnItemNum == key + "-RET", ct);
            if (returnOrder == null)
            {
                Success(await returns.CreateAsync(new(key + "-I1", key + "-RET", [new(original.RecId, 10m)]), ct), "create return");
                returnOrder = await db.Set<PurchTable>().SingleAsync(x => x.DataAreaId == Area && x.ReturnItemNum == key + "-RET", ct);
            }
            var returnLine = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == Area && x.PurchId == returnOrder.PurchId, ct);
            if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.PurchId == returnOrder.PurchId, ct))
                Success(await returns.ShipAsync(returnOrder.RecId, new(key + "-RS", date.AddDays(1), [new(returnLine.RecId, 10m)]), ct), "return shipment");
            if (!await HasInvoice(key + "-CR", account, ct)) Success(await returns.CreditAsync(returnOrder.RecId, new(key + "-CR", date.AddDays(2)), ct), "vendor credit");
            var credit = await db.Set<VendInvoiceJour>().SingleAsync(x => x.DataAreaId == Area && x.InvoiceAccount == account && x.InvoiceId == key + "-CR", ct);
            paymentAmount += credit.InvoiceAmount;
            await SettleMarked(key, account, date.AddDays(3), ct);
            expectedPhysical -= 10m;
            steps.Add(new("Return and credit", returnOrder.PurchId, "10 units shipped and credited from original invoice"));
        }
        var requestId = key + "-PAY";
        Success(await payments.PostAsync(new(account, id == "13" ? "APV2-USD" : "APV2-BANK", id == "08" ? 1000m : paymentAmount, requestId,
            id == "12" ? date.AddDays(5) : date.AddDays(35), SettleInvoiceId: id == "09" || posted.Count != 1 ? null : posted[0].InvoiceId,
            SettleAutomatically: id != "09" && posted.Count == 1), ct), "vendor payment");
        steps.Add(new("Payment", requestId, id == "09" ? "Posted on account before separate settlement" : "Posted with settlement"));
        if (id == "08") expectedOpenCredit = total - 1000m;
        if (id == "09" || posted.Count > 1) await SettleMarked(key, account, date.AddDays(36), ct);
        var paymentLine = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.DataAreaId == Area && x.PaymReference == requestId && x.AmountCurDebit > 0m, ct);
        if (id == "18")
        {
            var active = await db.Set<VendSettlement>().AnyAsync(x => x.DataAreaId == Area && x.Voucher == paymentLine.Voucher && x.CanBeReversed == NoYes.Yes, ct);
            if (active) Success(await undoSettlement.ReverseAsync(paymentLine.Voucher, date.AddDays(36), ct), "settlement reversal");
            expectedOpenCredit = total; expectedOpenDebit = total;
            steps.Add(new("Settlement reversal", paymentLine.Voucher, "Invoice and payment reopened"));
        }
        if (id == "19")
        {
            if (!await db.Set<LedgerJournalTable>().AnyAsync(x => x.DataAreaId == Area && x.OriginalJournalNum == paymentLine.JournalNum && x.Posted == NoYes.Yes, ct))
                Success(await undoPayment.ReverseAsync(paymentLine.JournalNum, date.AddDays(36), ct), "payment reversal");
            expectedOpenCredit = total;
            steps.Add(new("Payment reversal", paymentLine.JournalNum, "Bank/payment reversed; invoice reopened"));
        }
        return await Reconcile(key, id, expectedPhysical, expectedOpenDebit, expectedOpenCredit, ct);
    }
    private async Task EnsureLine(PurchTable order, string item, decimal qty, decimal price, DateTime date, CancellationToken ct)
    {
        var existing = await db.Set<PurchLine>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId && x.ItemId == item, ct);
        if (existing != null) { Check(existing.PurchQty == qty && existing.PurchPrice == price, "existing example line differs."); return; }
        Success(await orders.AddLineAsync(order.RecId, new(item, qty, "EA", price, 1m, "APV2-SITE", "APV2-WH", date, null), ct), "add purchase line");
    }
    private async Task Receipt(PurchTable order, string number, DateTime date, List<PurchPostLineDto> lines, CancellationToken ct)
    {
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId && x.PackingSlipId == number, ct))
            Success(await receipts.PostAsync(order.RecId, new(number, date, lines), ct), "product receipt");
        steps.Add(new("Product receipt", number, "Posted/verified"));
    }
    private Task<bool> HasInvoice(string number, string account, CancellationToken ct) =>
        db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == Area && x.InvoiceAccount == account && x.InvoiceId == number, ct);
    private async Task Invoice(PurchTable order, string number, DateTime date, List<PurchPostLineDto> lines, CancellationToken ct)
    {
        if (!await HasInvoice(number, order.InvoiceAccount, ct))
        {
            var held = await db.Set<VendInvoiceInfoTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.InvoiceAccount == order.InvoiceAccount && x.Num == number, ct);
            if (held == null)
            {
                var result = await pending.CreateFromOrderAsync(order.RecId, new(number, date, date, "APV2 real example", lines), ct);
                Success(result, "hold pending invoice");
                held = await db.Set<VendInvoiceInfoTable>().SingleAsync(x => x.DataAreaId == Area && x.Num == number && x.InvoiceAccount == order.InvoiceAccount, ct);
            }
            var heldLines = await db.Set<VendInvoiceInfoLine>().Where(x => x.DataAreaId == Area && x.ParmId == held.ParmId).Select(x => x.RecId).ToArrayAsync(ct);
            Check(await db.Set<VendInvoiceInfoSubLine>().AnyAsync(x => x.DataAreaId == Area && x.LineRefRecId != null && heldLines.Contains(x.LineRefRecId.Value), ct), "pending invoice has no receipt links.");
            steps.Add(new("Pending invoice", held.ParmId, "Held with product-receipt line links; no invoice posting yet"));
            Success(await pending.PostAsync(held.ParmId, ct), "post pending invoice");
        }
        steps.Add(new("Invoice", number, "Posted/verified"));
    }
    private async Task SettleMarked(string key, string account, DateTime date, CancellationToken ct)
    {
        var invoiceIds = await db.Set<VendTrans>().Where(x => x.DataAreaId == Area && x.AccountNum == account && x.Invoice.StartsWith(key + "-")).Select(x => x.RecId).ToArrayAsync(ct);
        var requestLines = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == Area && x.PaymReference.StartsWith(key + "-")).Select(x => x.Voucher).ToArrayAsync(ct);
        var paymentIds = await db.Set<VendTrans>().Where(x => x.DataAreaId == Area && requestLines.Contains(x.Voucher)).Select(x => x.RecId).ToArrayAsync(ct);
        var ids = invoiceIds.Concat(paymentIds).ToArray();
        var open = await db.Set<VendTransOpen>().Where(x => x.DataAreaId == Area && ids.Contains(x.RefRecId)).ToListAsync(ct);
        if (open.Any(x => x.AmountCur > 0) && open.Any(x => x.AmountCur < 0))
            Success(await settlements.SettleAsync(new(account, date, open.Select(x => x.RefRecId).ToArray()), ct), "settle open transactions");
        steps.Add(new("Separate settlement", key, "Marked transactions settled/verified"));
    }
    private async Task<string> PostingCounts(CancellationToken ct) => string.Join(":", await db.Set<VendTrans>().CountAsync(ct),
        await db.Set<GeneralJournalEntry>().CountAsync(ct), await db.Set<GeneralJournalAccountEntry>().CountAsync(ct),
        await db.Set<AccountingDistribution>().CountAsync(ct), await db.Set<SubledgerJournalEntry>().CountAsync(ct),
        await db.Set<SourceDocumentHeader>().CountAsync(ct), await db.Set<SourceDocumentLine>().CountAsync(ct));
    private async Task<AccountsPayableScenarioReport> Reconcile(string key, string id, decimal expectedPhysical,
        decimal expectedDebit, decimal expectedCredit, CancellationToken ct)
    {
        var purchaseOrders = await db.Set<PurchTable>().Where(x => x.DataAreaId == Area && (x.PurchName == key || x.ReturnItemNum == key + "-RET")).Select(x => x.PurchId).ToArrayAsync(ct);
        var invoiceRows = await db.Set<VendInvoiceJour>().Where(x => x.DataAreaId == Area && x.InvoiceId.StartsWith(key + "-")).ToListAsync(ct);
        var paymentLines = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == Area && x.PaymReference.StartsWith(key + "-")).ToListAsync(ct);
        var journals = paymentLines.Select(x => x.JournalNum).ToArray();
        var reversals = await db.Set<LedgerJournalTable>().Where(x => x.DataAreaId == Area && journals.Contains(x.OriginalJournalNum)).Select(x => x.JournalNum).ToArrayAsync(ct);
        var reverseVouchers = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == Area && reversals.Contains(x.JournalNum)).Select(x => x.Voucher).ToArrayAsync(ct);
        var vendorVouchers = invoiceRows.Select(x => x.LedgerVoucher).Concat(paymentLines.Select(x => x.Voucher)).Concat(reverseVouchers).ToArray();
        var vendorTransactions = await db.Set<VendTrans>().Where(x => x.DataAreaId == Area && vendorVouchers.Contains(x.Voucher)).ToListAsync(ct);
        var ids = vendorTransactions.Select(x => x.RecId).ToArray();
        var open = await db.Set<VendTransOpen>().Where(x => x.DataAreaId == Area && ids.Contains(x.RefRecId)).ToListAsync(ct);
        var debit = open.Where(x => x.AmountCur > 0).Sum(x => x.AmountCur);
        var credit = -open.Where(x => x.AmountCur < 0).Sum(x => x.AmountCur);
        Check(debit == expectedDebit && credit == expectedCredit, $"{id}: open balances differ ({debit}/{credit}).");
        var origins = await db.Set<InventTransOrigin>().Where(x => x.DataAreaId == Area && purchaseOrders.Contains(x.ReferenceId) && x.ReferenceCategory == InventRefType.PurchaseOrder).Select(x => x.RecId).ToArrayAsync(ct);
        var inventory = await db.Set<InventTrans>().Where(x => x.DataAreaId == Area && origins.Contains(x.InventTransOrigin)).ToListAsync(ct);
        var physical = inventory.Where(x => x.StatusReceipt is StatusReceipt.Received or StatusReceipt.Purchased
            || x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold).Sum(x => x.Qty);
        Check(physical == expectedPhysical, $"{id}: physical quantity differs ({physical}).");
        var receiptVouchers = await db.Set<VendPackingSlipJour>().Where(x => x.DataAreaId == Area && purchaseOrders.Contains(x.PurchId)).Select(x => x.LedgerVoucher).ToArrayAsync(ct);
        var settlementRows = await db.Set<VendSettlement>().Where(x => x.DataAreaId == Area && ids.Contains(x.InvoiceTransRecId)).ToListAsync(ct);
        var settlementVouchers = settlementRows.Select(x => x.Voucher).ToArray();
        var allVouchers = vendorVouchers.Concat(receiptVouchers).Concat(settlementVouchers).Distinct().ToArray();
        var posted = await db.Set<GeneralJournalEntry>().Where(x => x.DataAreaId == Area && allVouchers.Contains(x.SubledgerVoucher)).Select(x => x.RecId).ToArrayAsync(ct);
        var entries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == Area && posted.Contains(x.GeneralJournalEntry)).ToListAsync(ct);
        Check(entries.GroupBy(x => x.GeneralJournalEntry).All(g => g.Sum(x => x.AccountingCurrencyAmount) == 0m && g.Sum(x => x.ReportingCurrencyAmount) == 0m), "voucher does not balance.");
        var vendorDimension = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.DataAreaId == Area && x.MainAccountValue == "281001", ct);
        Check(entries.Where(x => x.LedgerDimension == vendorDimension.RecId).Sum(x => x.AccountingCurrencyAmount) == vendorTransactions.Sum(x => x.AmountMst + x.VendExchAdjustmentRealized)
            + settlementRows.Sum(x => x.UtilizedCashDisc * (vendorTransactions.Single(t => t.RecId == x.PaymentTransRecId).AmountCur == 0 ? 1m :
                vendorTransactions.Single(t => t.RecId == x.PaymentTransRecId).AmountMst / vendorTransactions.Single(t => t.RecId == x.PaymentTransRecId).AmountCur)), "vendor subledger does not match GL control account.");
        var accruals = await (from accrual in db.Set<PurchReceiptAccrual>() join receipt in db.Set<VendPackingSlipTrans>() on accrual.PackingSlipTransRecId equals receipt.RecId
            where accrual.DataAreaId == Area && receipt.DataAreaId == Area && purchaseOrders.Contains(receipt.OrigPurchId) select accrual).ToListAsync(ct);
        Check(accruals.All(x => x.ReversedInventQuantity == x.InventQuantity && x.ReversedAmountMst == x.AmountMst), "fully invoiced receipt accrual remains open.");
        var tax = await db.Set<TaxTrans>().Where(x => x.DataAreaId == Area && vendorVouchers.Contains(x.Voucher)).ToListAsync(ct);
        var bank = await db.Set<BankAccountTrans>().Where(x => x.DataAreaId == Area && vendorVouchers.Contains(x.Voucher)).ToListAsync(ct);
        var dimensions = await db.Set<DimensionAttributeValueCombination>().Where(x => x.DataAreaId == Area).ToListAsync(ct);
        decimal LedgerAmount(string code) => entries.Where(x => dimensions.Any(d => d.RecId == x.LedgerDimension && d.MainAccountValue == code)).Sum(x => x.AccountingCurrencyAmount);
        Check(LedgerAmount("181001") == bank.Sum(x => x.AmountMst), $"{id}: bank movements do not match GL.");
        Check(LedgerAmount("181004") == tax.Sum(x => x.TaxAmount), $"{id}: input tax does not match GL.");
        Check(LedgerAmount("181002") == inventory.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment), $"{id}: financial inventory does not match GL.");
        Check(LedgerAmount("181003") == 0m && LedgerAmount("281002") == 0m, $"{id}: receipt interim accounts remain open.");
        Check(LedgerAmount("181005") == 0m, $"{id}: applied vendor advance remains in GL.");
        var entryIds = entries.Select(e => e.RecId).ToArray();
        var links = await db.Set<SubledgerJournalAccountEntry>().Where(x => x.GeneralJournalAccountEntry != null && entryIds.Contains(x.GeneralJournalAccountEntry.Value)).CountAsync(ct);
        return new(id, Scenarios[id], steps.ToArray(), purchaseOrders, vendorTransactions.Where(x => x.Invoice.Length > 0).Select(x => x.Invoice).Distinct().ToArray(),
            debit - credit, debit, credit, physical, bank.Sum(x => x.AmountCur), tax.Sum(x => x.TaxAmount), posted.Length, links);
    }
}
