using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;

/// <summary>
/// The sales cycle, scenario by scenario, each document committed by its owning business service.
/// Every step resolves what already exists before posting again, so a rerun resumes instead of
/// duplicating; nothing here writes a document, an inventory movement or a ledger entry directly.
/// </summary>
public sealed class AccountsReceivableTransactionFlow(ApplicationDbContext db,
    ISalesOrderQuickCreateService orders, ISalesOrderLineService lines,
    ISalesOrderConfirmationService confirmations, IPackingSlipJournalService deliveries,
    ISalesInvoiceService invoices, IFreeTextInvoiceService freeText, ICustPaymentService payments,
    ICustSettleOpenTransactionsService settlements, ICustReturnService returns,
    ICustSettlementReversalService undoSettlement, ICustPaymentReversalService undoPayment,
    ISalesOrderCancellationService cancellations)
{
    public static IReadOnlyDictionary<string, string> Scenarios { get; } = new Dictionary<string, string>
    {
        ["01"] = "Stock and service sale to collection", ["02"] = "Partial deliveries and invoices",
        ["03"] = "Non-stocked service sales order", ["04"] = "Free text invoice",
        ["05"] = "Invoice across multiple deliveries", ["08"] = "Partial collection",
        ["09"] = "On-account collection and later settlement", ["12"] = "Cash discount",
        ["13"] = "Foreign-currency sale and realized exchange difference",
        ["14"] = "Sales return and customer credit", ["16"] = "Cancel undelivered remainder",
        ["18"] = "Settlement reversal", ["19"] = "Payment reversal",
        ["20"] = "Validation failures and rollback"
    };

    private string Area => db.GetDataAreaId();
    private readonly List<AccountsReceivableStageReport> steps = [];

    private static void Check(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException("AR cycle integrity: " + message);
    }

    private static void Success(int statusCode, string? message, string stage)
    {
        if (statusCode != 200) throw new InvalidOperationException($"AR {stage} failed ({statusCode}): {message}");
    }

    public async Task<AccountsReceivableScenarioReport> ExecuteAsync(AccountsReceivableTransactionOptions options, string id, CancellationToken ct)
    {
        steps.Clear();
        var key = $"ARV2-{options.FiscalYear}-{id}";
        var date = new DateTime(options.FiscalYear, 4, 15);
        var account = $"ARV2-{Area}-{(id == "13" ? "USD" : "LOCAL")}";
        var currency = id == "13" ? "USD" : "SAR";

        if (id == "04")
        {
            await FreeTextAsync(key, account, date, ct);
            var invoiced = await db.Set<CustInvoiceJour>()
                .Where(x => x.DataAreaId == Area && x.InvoiceAccount == account && x.InvoiceId.StartsWith(key + "-I"))
                .SumAsync(x => x.InvoiceAmount, ct);
            var freeTextCollection = await payments.PostAsync(new(account, "APV2-BANK", invoiced,
                date.AddDays(35), "ARV2", key + "-I1", key + "-PAY"), ct);
            Success(freeTextCollection.StatusCode, freeTextCollection.Message, "free text collection");
            return await Reconcile(key, id, 0m, 0m, 0m, ct);
        }

        var serviceOnly = id == "03";
        var order = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area
            && x.SalesName == key && x.ReturnItemNum == string.Empty, ct);
        if (order == null)
        {
            var created = await orders.CreateAsync(new SalesOrderQuickCreateDto
            {
                CustomerAccount = account, CurrencyCode = currency, SalesName = key,
                InventSiteId = "APV2-SITE", InventLocationId = "APV2-WH",
                RequestedShipDate = date, RequestedReceiptDate = date
            }, ct);
            Check(created.Error == null && created.Order != null, created.Error ?? "The example sales order was not created.");
            order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == Area && x.RecId == created.Order!.RecId, ct);
        }
        Check(order.CustAccount == account && order.CurrencyCode == currency, "existing example order differs.");

        await EnsureLine(order, serviceOnly ? ExampleIdentifiers.ServiceItem : ExampleIdentifiers.StockItem,
            serviceOnly ? 2m : 100m, serviceOnly ? 500m : id == "13" ? 11m : 40m, date, ct);
        if (id == "01") await EnsureLine(order, ExampleIdentifiers.ServiceItem, 2m, 500m, date, ct);

        if (!await db.Set<CustConfirmJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId, ct))
        {
            var confirmed = await confirmations.PostAsync(order.RecId, new(date.AddDays(-12), true, key + "-CONF"), ct);
            Success(confirmed.StatusCode, confirmed.Message, "confirm order");
        }
        steps.Add(new("Sales order and confirmation", order.SalesId, "Created/verified"));

        var orderLines = (await db.Set<SalesLine>().Where(x => x.DataAreaId == Area && x.SalesId == order.SalesId).ToListAsync(ct))
            .OrderBy(x => x.LineNum).ToList();

        if (id == "02")
        {
            await Deliver(order, key + "-D1", date.AddDays(-10), [(orderLines[0].RecId, 60m)], ct);
            await Invoice(order, key + "-I1", date.AddDays(-8), [(orderLines[0].RecId, 40m)], ct);
            await Deliver(order, key + "-D2", date.AddDays(-5), [(orderLines[0].RecId, 40m)], ct);
            await Invoice(order, key + "-I2", date, [(orderLines[0].RecId, 60m)], ct);
        }
        else if (id == "05")
        {
            await Deliver(order, key + "-D1", date.AddDays(-10), [(orderLines[0].RecId, 60m)], ct);
            await Deliver(order, key + "-D2", date.AddDays(-5), [(orderLines[0].RecId, 40m)], ct);
            await Invoice(order, key + "-I1", date, [(orderLines[0].RecId, 100m)], ct);
        }
        else
        {
            var quantities = orderLines.Select(x => (x.RecId, id is "16" or "20" ? 60m : x.SalesQty)).ToList();
            await Deliver(order, key + "-D1", date.AddDays(-5), quantities, ct);
            if (id == "16" && orderLines.Any(x => x.RemainSalesPhysical > 0))
            {
                var cancelled = await cancellations.CancelAsync(order.RecId, ct);
                Check(cancelled.Status is SalesOrderCancelStatus.Cancelled or SalesOrderCancelStatus.NothingToCancel,
                    cancelled.Message ?? "the undelivered remainder was not cancelled.");
                steps.Add(new("Cancel remainder", order.SalesId, "Undelivered quantity canceled; delivered quantity preserved"));
            }
            if (id == "20" && !await HasInvoice(key + "-I1", account, ct))
            {
                var before = await PostingCounts(ct);
                var excess = await invoices.PostAsync(order.RecId,
                    new(date, key + "-BAD", [new(orderLines[0].RecId, 101m)]), ct);
                Check(excess.StatusCode is 400 or 422, "invoicing more than was delivered was not rejected.");
                Check(before == await PostingCounts(ct), "the rejected invoice left posting rows behind.");
                steps.Add(new("Negative validation", key, "Excess quantity rejected without posting"));
            }
            await Invoice(order, key + "-I1", date, quantities, ct);
        }

        var posted = await db.Set<CustInvoiceJour>().Where(x => x.DataAreaId == Area && x.InvoiceAccount == account
            && x.InvoiceId.StartsWith(key + "-I")).ToListAsync(ct);
        var total = posted.Sum(x => x.InvoiceAmount);
        decimal expectedPhysical = serviceOnly ? 0m : id is "16" or "20" ? -60m : -100m;
        decimal expectedOpenCredit = 0m, expectedOpenDebit = 0m;
        var collected = total;

        if (id == "12") collected = decimal.Round(total * 0.98m, 2, MidpointRounding.AwayFromZero);
        if (id == "14")
        {
            var sold = await db.Set<CustInvoiceTrans>().SingleAsync(x => x.DataAreaId == Area
                && x.InvoiceId == key + "-I1" && x.ItemId == ExampleIdentifiers.StockItem, ct);
            var returnOrder = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.ReturnItemNum == key + "-RET", ct);
            if (returnOrder == null)
            {
                var opened = await returns.CreateAsync(new(key + "-I1", null, [new(sold.RecId, 10m, "ARV2-RETURN")]), ct);
                Success(opened.StatusCode, opened.Message, "create return");
                returnOrder = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == Area && x.ReturnItemNum == key + "-RET", ct);
            }
            var returnLine = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == Area && x.SalesId == returnOrder.SalesId, ct);
            if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == returnOrder.SalesId, ct))
            {
                var received = await returns.ReceiveAsync(returnOrder.RecId, new(key + "-RR", date.AddDays(1), [new(returnLine.RecId, 10m)]), ct);
                Success(received.StatusCode, received.Message, "return receipt");
            }
            if (!await HasInvoice(key + "-CR", account, ct))
            {
                var credited = await returns.CreditAsync(returnOrder.RecId, new(date.AddDays(2), key + "-CR"), ct);
                Success(credited.StatusCode, credited.Message, "customer credit note");
            }
            var credit = await db.Set<CustInvoiceJour>().SingleAsync(x => x.DataAreaId == Area
                && x.InvoiceAccount == account && x.InvoiceId == key + "-CR", ct);
            collected += credit.InvoiceAmount;
            await SettleMarked(key, account, date.AddDays(3), ct);
            expectedPhysical += 10m;
            steps.Add(new("Return and credit", returnOrder.SalesId, "10 units received back and credited from the original invoice"));
        }

        var requestId = key + "-PAY";
        var collection = await payments.PostAsync(new(account, id == "13" ? "APV2-USD" : "APV2-BANK",
            id == "08" ? 1000m : collected, id == "12" ? date.AddDays(5) : date.AddDays(35), "ARV2",
            id == "09" || posted.Count != 1 ? null : posted[0].InvoiceId, requestId), ct);
        Success(collection.StatusCode, collection.Message, "customer collection");
        steps.Add(new("Collection", requestId, id == "09" ? "Posted on account before separate settlement" : "Posted with settlement"));
        if (id == "08") expectedOpenDebit = total - 1000m;
        if (id == "09" || posted.Count > 1) await SettleMarked(key, account, date.AddDays(36), ct);

        var paymentLine = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.DataAreaId == Area
            && x.PaymReference == requestId && x.AmountCurCredit > 0m, ct);
        if (id == "18")
        {
            var settlementVoucher = await db.Set<CustSettlement>().Where(x => x.DataAreaId == Area
                && x.OffsetTransVoucher == paymentLine.Voucher && x.CanBeReversed == NoYes.Yes)
                .Select(x => x.SettlementVoucher).FirstOrDefaultAsync(ct);
            if (settlementVoucher != null)
            {
                var reversed = await undoSettlement.ReverseAsync(settlementVoucher, date.AddDays(36), ct);
                Success(reversed.StatusCode, reversed.Message, "settlement reversal");
            }
            expectedOpenDebit = total; expectedOpenCredit = total;
            steps.Add(new("Settlement reversal", paymentLine.Voucher, "Invoice and collection reopened"));
        }
        if (id == "19")
        {
            if (!await db.Set<LedgerJournalTable>().AnyAsync(x => x.DataAreaId == Area
                && x.OriginalJournalNum == paymentLine.JournalNum && x.Posted == NoYes.Yes, ct))
            {
                var reversed = await undoPayment.ReverseAsync(paymentLine.JournalNum, date.AddDays(36), ct);
                Success(reversed.StatusCode, reversed.Message, "payment reversal");
            }
            expectedOpenDebit = total;
            steps.Add(new("Payment reversal", paymentLine.JournalNum, "Bank/collection reversed; invoice reopened"));
        }

        return await Reconcile(key, id, expectedPhysical, expectedOpenDebit, expectedOpenCredit, ct);
    }

    /// <summary>A direct invoice with no sales order and no inventory movement.</summary>
    private async Task FreeTextAsync(string key, string account, DateTime date, CancellationToken ct)
    {
        if (await HasInvoice(key + "-I1", account, ct)) { steps.Add(new("Free text invoice", key + "-I1", "Posted/verified")); return; }
        var revenue = await db.Set<DimensionAttributeValueCombination>()
            .SingleAsync(x => x.DataAreaId == Area && x.MainAccountValue == "481003", ct);
        var draft = await db.Set<CustInvoiceTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.InvoiceId == key + "-I1", ct);
        long recId;
        if (draft == null)
        {
            var created = await freeText.CreateAsync(new FreeTextInvoiceSaveDto
            {
                CustomerAccount = account, InvoiceDate = date, CurrencyCode = "SAR", TaxGroup = "ARV2",
                Description = "ARV2 direct service invoice example",
                Lines = [new FreeTextInvoiceLineInputDto { Description = "Advisory services (demo)", Quantity = 1m,
                    UnitPrice = 350m, LedgerDimension = revenue.RecId, TaxGroup = "ARV2", TaxItemGroup = "APV2" }]
            }, ct);
            Success(created.StatusCode, created.Message, "create free text invoice");
            recId = created.Invoice!.RecId;
        }
        else recId = draft.RecId;
        var postedResult = await freeText.PostAsync(recId, ct);
        Success(postedResult.StatusCode, postedResult.Message, "post free text invoice");
        steps.Add(new("Free text invoice", key + "-I1", "Posted through its own service; no inventory movement"));
    }

    private async Task EnsureLine(SalesTable order, string item, decimal qty, decimal price, DateTime date, CancellationToken ct)
    {
        var existing = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == Area
            && x.SalesId == order.SalesId && x.ItemId == item, ct);
        if (existing != null) { Check(existing.SalesQty == qty, "existing example line differs."); return; }
        var added = await lines.AddAsync(order.RecId, new SalesLineInputDto
        {
            ItemNumber = item, Quantity = qty, UnitPrice = price, Unit = "EA", PriceUnit = 1m,
            InventSiteId = "APV2-SITE", InventLocationId = "APV2-WH", DeliveryDate = date,
            TaxGroup = "ARV2", TaxItemGroup = "APV2"
        }, ct);
        Check(added.Status == SalesLineWriteStatus.Success, added.Error ?? "the example sales line was not added.");
    }

    private async Task Deliver(SalesTable order, string number, DateTime date, List<(long RecId, decimal Qty)> quantities, CancellationToken ct)
    {
        if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area
            && x.SalesId == order.SalesId && x.PackingSlipId == number, ct))
        {
            var result = await deliveries.PostAsync(order.RecId, new(date, "all", true, number,
                quantities.Select(x => new PackingSlipPostingLineRequest(
                    x.RecId.ToString(CultureInfo.InvariantCulture), x.Qty)).ToArray()), ct);
            Success(result.StatusCode, result.Message, "packing slip");
        }
        steps.Add(new("Delivery", number, "Posted/verified"));
    }

    private Task<bool> HasInvoice(string number, string account, CancellationToken ct) =>
        db.Set<CustInvoiceJour>().AnyAsync(x => x.DataAreaId == Area && x.InvoiceAccount == account && x.InvoiceId == number, ct);

    private async Task Invoice(SalesTable order, string number, DateTime date, List<(long RecId, decimal Qty)> quantities, CancellationToken ct)
    {
        if (!await HasInvoice(number, order.InvoiceAccount, ct))
        {
            var result = await invoices.PostAsync(order.RecId, new(date, number,
                quantities.Select(x => new SalesInvoiceLineDto(x.RecId, x.Qty)).ToList()), ct);
            Success(result.StatusCode, result.Message, "sales invoice");
        }
        steps.Add(new("Invoice", number, "Posted/verified"));
    }

    private async Task SettleMarked(string key, string account, DateTime date, CancellationToken ct)
    {
        var invoiceIds = await db.Set<CustTrans>().Where(x => x.DataAreaId == Area && x.AccountNum == account
            && x.Invoice.StartsWith(key + "-")).Select(x => x.RecId).ToArrayAsync(ct);
        var requestLines = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == Area
            && x.PaymReference.StartsWith(key + "-")).Select(x => x.Voucher).ToArrayAsync(ct);
        var paymentIds = await db.Set<CustTrans>().Where(x => x.DataAreaId == Area && requestLines.Contains(x.Voucher))
            .Select(x => x.RecId).ToArrayAsync(ct);
        var ids = invoiceIds.Concat(paymentIds).ToArray();
        var open = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Area && ids.Contains(x.RefRecId)).ToListAsync(ct);
        if (open.Any(x => x.AmountCur > 0) && open.Any(x => x.AmountCur < 0))
        {
            var settled = await settlements.SettleAsync(new(account, date, open.Select(x => x.RefRecId).ToArray()), ct);
            Success(settled.StatusCode, settled.Message, "settle open transactions");
        }
        steps.Add(new("Separate settlement", key, "Marked transactions settled/verified"));
    }

    /// <summary>A fingerprint of every posting table, so a rejected operation can be proven to have written nothing.</summary>
    private async Task<string> PostingCounts(CancellationToken ct) => string.Join(":",
        await db.Set<CustTrans>().CountAsync(ct), await db.Set<GeneralJournalEntry>().CountAsync(ct),
        await db.Set<GeneralJournalAccountEntry>().CountAsync(ct), await db.Set<AccountingDistribution>().CountAsync(ct),
        await db.Set<SubledgerJournalEntry>().CountAsync(ct), await db.Set<SourceDocumentHeader>().CountAsync(ct),
        await db.Set<SourceDocumentLine>().CountAsync(ct));

    private async Task<AccountsReceivableScenarioReport> Reconcile(string key, string id, decimal expectedPhysical,
        decimal expectedDebit, decimal expectedCredit, CancellationToken ct)
    {
        var posted = await AccountsReceivableReconciliationSnapshot.LoadAsync(db, key, id, expectedPhysical, expectedDebit, expectedCredit, ct);
        foreach (var check in AccountsReceivableReconciliationSnapshot.Checks) check.Verify(posted);
        return new(id, Scenarios[id], steps.ToArray(), posted.SalesOrders,
            posted.CustomerTransactions.Where(x => x.Invoice.Length > 0).Select(x => x.Invoice).Distinct().ToArray(),
            posted.OpenDebit - posted.OpenCredit, posted.OpenDebit, posted.OpenCredit, posted.PhysicalQuantity,
            posted.Bank.Sum(x => x.AmountCur), posted.Tax.Sum(x => x.TaxAmount), posted.PostedVouchers.Count, posted.SourceDistributions);
    }
}
