using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;

/// <summary>
/// GL-02, the fifteen-step reference cycle of <c>Logic_Readonly.md</c> §2: buy 100 at 50, sell 30 at 90 plus a
/// 300 service in two deliveries and invoices, collect 2,000 then 1,000, pay the vendor 2,000, close inventory
/// cost and accrue a 300 expense. Every document is posted by its owning service; nothing is written directly.
/// The §49 trial balance (6,300 each side, net profit 1,200) is then checked over this scenario's own vouchers,
/// because the seeded company carries the other examples' postings too. The §5 year-end close is not run here:
/// it would close the year for every example in the company, so it stays proven by the GL cycle test.
/// </summary>
public sealed class GeneralLedgerReferenceCycleFlow(ApplicationDbContext db, IPurchOrderService purchases,
    IPurchConfirmationService confirmations, IProductReceiptService receipts, IVendorInvoiceService vendorInvoices,
    ISalesOrderQuickCreateService sales, ISalesOrderLineService salesLines, IPackingSlipJournalService deliveries,
    ISalesInvoiceService customerInvoices, IInventoryReservationService reservations, ICustPaymentService custPayments,
    ICustSettleOpenTransactionsService custSettlements, IVendPaymentService vendPayments, IInventCloseService closing,
    ILedgerJournalTableService journals, ILedgerJournalTransService journalLines)
{
    public const decimal TrialBalance = 6300m;
    public const decimal NetProfit = 1200m;
    private const string Bank = "APV2-BANK";
    private string Area => db.GetDataAreaId();

    private static void Check(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException("GL reference cycle: " + message);
    }

    private static void Success(int status, string? error) => Check(status is 200 or 201, error ?? $"posting failed ({status}).");

    public async Task<GeneralLedgerScenarioReport> RunAsync(int fiscalYear, CancellationToken ct = default)
    {
        if (db.ChangeTracker.HasChanges()) throw new InvalidOperationException("Run GL examples in a clean DI scope.");
        db.ChangeTracker.Clear();
        await using var seedLock = await new SqlServerSeederLock(db).AcquireLockAsync(120, ct);

        var key = $"GLV2-{fiscalYear}-02";
        var item = InventoryCycleMasterSeeder.Item(fiscalYear, InventoryCycleSeedExample.GlReferenceSuffix);
        var service = ExampleIdentifiers.ServiceItem(fiscalYear);
        var vendor = ExampleIdentifiers.Vendor(Area);
        var customer = ExampleIdentifiers.Customer(Area);
        var day = new DateTime(fiscalYear, 2, 1);

        // Steps 1-3: purchase 100 at 50, receive, invoice. Inventory 5,000 against the vendor.
        var purchase = await BuyAsync(key, vendor, item, day, ct);

        // Step 4: one order with the stocked item (30 at 90) and the 300 service.
        var order = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.SalesName == key, ct);
        if (order is null)
        {
            var created = await sales.CreateAsync(new SalesOrderQuickCreateDto
            {
                CustomerAccount = customer, CurrencyCode = "SAR", SalesName = key,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse,
                RequestedShipDate = day.AddDays(4), RequestedReceiptDate = day.AddDays(4)
            }, ct);
            Check(created.Error is null && created.Order is not null, created.Error ?? "sales order creation failed.");
            order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == Area && x.RecId == created.Order!.RecId, ct);
        }
        Check(order.CustAccount == customer && order.CurrencyCode == "SAR", "existing sales order differs.");
        var stockLine = await EnsureSalesLineAsync(order, item, 30m, 90m, day.AddDays(3), ct);
        var serviceLine = await EnsureSalesLineAsync(order, service, 1m, 300m, day.AddDays(3), ct);

        // Step 5: reserve the 30 - on hand stays 100, available drops to 70.
        if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId, ct))
        {
            var reserved = await reservations.ReserveAsync(stockLine.InventTransId, 30m, false, ct);
            Check(reserved.Error is null, reserved.Error ?? "reservation failed.");
            db.ChangeTracker.Clear();
            var sum = await db.Set<InventSum>().SingleAsync(x => x.DataAreaId == Area && x.ItemId == item, ct);
            Check(sum.PhysicalInvent == 100m && sum.AvailPhysical == 70m, "reservation quantities differ.");
        }

        // Steps 6-9: deliver and invoice 20 (customer 1,800), then the last 10 with the service (customer 1,200).
        await DeliverAsync(order, key + "-D1", stockLine, 20m, day.AddDays(5), ct);
        var first = await InvoiceAsync(order, key + "-I1", [new(stockLine.RecId, 20m)], day.AddDays(6), ct);
        await DeliverAsync(order, key + "-D2", stockLine, 10m, day.AddDays(7), ct);
        var second = await InvoiceAsync(order, key + "-I2", [new(stockLine.RecId, 10m), new(serviceLine.RecId, 1m)], day.AddDays(8), ct);
        Check(first.InvoiceAmount == 1800m && second.InvoiceAmount == 1200m, "sales invoices are not 1,800 and 1,200.");
        Check(!await db.Set<InventTrans>().AnyAsync(x => x.DataAreaId == Area && x.ItemId == service
            && x.InvoiceId == second.InvoiceId, ct), "the service line produced an inventory transaction.");

        // Steps 10-12: collect 2,000 (settles 1,800 and 200), then 1,000. Settled explicitly against this
        // scenario's two invoices, because the shared demo customer carries other examples' open invoices.
        var invoiceIds = new[] { first.InvoiceId, second.InvoiceId };
        await CollectAsync(customer, key + "-C1", 2000m, day.AddDays(9), invoiceIds, ct);
        await CollectAsync(customer, key + "-C2", 1000m, day.AddDays(11), invoiceIds, ct);
        Check(await OpenCustomerAsync(customer, invoiceIds, ct) == 0m, "the scenario's customer invoices are still open.");

        // Step 13: pay the vendor 2,000 against the invoice, leaving 3,000 open.
        if (!await db.Set<LedgerJournalTrans>().AnyAsync(x => x.DataAreaId == Area && x.PaymReference == key + "-VP", ct))
            Success((await vendPayments.PostAsync(new(vendor, Bank, 2000m, key + "-VP", day.AddDays(12),
                SettleInvoiceId: purchase.InvoiceId), ct)).StatusCode, "vendor payment failed.");
        var vendorOpen = await (from open in db.Set<VendTransOpen>()
                                join trans in db.Set<VendTrans>() on open.RefRecId equals trans.RecId
                                where open.DataAreaId == Area && trans.AccountNum == vendor && trans.Invoice == purchase.InvoiceId
                                select open.AmountCur).SumAsync(ct);
        Check(vendorOpen == -3000m, $"vendor invoice has {vendorOpen} open instead of -3,000.");

        // Step 14: close inventory cost. One purchase price, so no adjustment: 70 left at 50 is 3,500.
        var closeDate = new DateTime(fiscalYear, 2, 28);
        if (!await db.Set<InventClosing>().AnyAsync(x => x.DataAreaId == Area && x.ScopeItemId == item
                && x.TransDate == closeDate && x.Cancellation == NoYes.No && x.CancelClosing == NoYes.No, ct))
        {
            var closed = await closing.CloseItemAsync(item, closeDate, ct);
            Check(closed.Error is null, closed.Error ?? "inventory close failed.");
            db.ChangeTracker.Clear();
        }
        var movements = await db.Set<InventTrans>().Where(x => x.DataAreaId == Area && x.ItemId == item).ToListAsync(ct);
        Check(movements.Sum(x => x.CostAmountAdjustment) == 0m, "the close adjusted a single-price item.");
        Check(movements.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) == 3500m, "closing inventory value is not 3,500.");

        // Step 15: accrue a 300 expense through the general journal.
        var accrual = await AccrueAsync(key + "-ACR", closeDate, ct);

        // §49: the trial balance over this scenario's vouchers.
        var balances = await BalancesAsync(purchase, order.SalesId, [key + "-C1", key + "-C2", key + "-VP"], accrual, ct);
        var debit = balances.Values.Where(x => x > 0).Sum();
        var credit = balances.Values.Where(x => x < 0).Sum();
        Check(debit == TrialBalance && credit == -TrialBalance, $"trial balance is {debit} / {credit} instead of 6,300 each side.");
        Check(Balance(GeneralLedgerSeedExample.Bank) == 1000m && Balance(GeneralLedgerSeedExample.Inventory) == 3500m
            && Balance(GeneralLedgerSeedExample.CostOfGoodsSold) == 1500m && Balance(GeneralLedgerSeedExample.MaintenanceExpense) == 300m
            && Balance(GeneralLedgerSeedExample.VendorControl) == -3000m && Balance(GeneralLedgerSeedExample.SalesRevenue) == -3000m
            && Balance(GeneralLedgerSeedExample.ReceivedNotInvoiced) == -300m && Balance(GeneralLedgerSeedExample.CustomerControl) == 0m,
            "account balances differ from the reference.");
        var accountTypes = await db.Set<MainAccount>().Where(x => balances.Keys.Contains(x.MainAccountId) && x.DataAreaId == Area)
            .Select(x => new { x.MainAccountId, x.Type }).ToListAsync(ct);
        var result = accountTypes.Where(x => x.Type is MainAccountType.Revenue or MainAccountType.Expense)
            .Select(x => x.MainAccountId).Distinct().Sum(x => balances[x]);
        Check(result == -NetProfit, $"net result is {-result} instead of a 1,200 profit.");

        await new SeedExecutionHistory().RecordAsync(db, $"ERPV2.GL.{fiscalYear}.02", Area, ct);
        db.ChangeTracker.Clear();
        return new("02", accrual, string.Empty, TrialBalance);

        decimal Balance(string account) => balances.GetValueOrDefault(account);
    }

    /// <summary>Purchase, confirm, receive and invoice 100 at 50 through the AP services.</summary>
    private async Task<VendInvoiceJour> BuyAsync(string key, string vendor, string item, DateTime date, CancellationToken ct)
    {
        var order = await db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.PurchName == key, ct);
        if (order is null)
        {
            var created = await purchases.CreateAsync(new(vendor, null, "SAR", date, ExampleIdentifiers.Site, ExampleIdentifiers.Warehouse, key, date), ct);
            Success(created.StatusCode, created.Message);
            order = (PurchTable)created.Data!;
        }
        Check(order.OrderAccount == vendor && order.CurrencyCode == "SAR", "existing purchase order differs.");
        var line = await db.Set<PurchLine>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct);
        if (line is null)
        {
            var added = await purchases.AddLineAsync(order.RecId, new(item, 100m, ExampleIdentifiers.Unit, 50m, 1m, null, null, date, item), ct);
            Success(added.StatusCode, added.Message);
            line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct);
        }
        Check(line.ItemId == item && line.PurchQty == 100m && line.PurchPrice == 50m, "existing purchase line differs.");
        if (!await db.Set<VendPurchOrderJour>().AnyAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct))
            Success((await confirmations.ConfirmAsync(order.RecId, ct, date)).StatusCode, "purchase confirmation failed.");
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct))
            Success((await receipts.PostAsync(order.RecId, new(key + "-R", date.AddDays(1), [new(line.RecId, 100m)]), ct)).StatusCode,
                "product receipt failed.");
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == Area && x.InvoiceAccount == vendor && x.InvoiceId == key + "-VI", ct))
            Success((await vendorInvoices.PostAsync(order.RecId, new(key + "-VI", date.AddDays(2), [new(line.RecId, 100m)]), ct)).StatusCode,
                "vendor invoice failed.");
        db.ChangeTracker.Clear();
        var invoice = await db.Set<VendInvoiceJour>().SingleAsync(x => x.DataAreaId == Area && x.InvoiceAccount == vendor && x.InvoiceId == key + "-VI", ct);
        Check(invoice.InvoiceAmount == 5000m, $"vendor invoice is {invoice.InvoiceAmount} instead of 5,000.");
        return invoice;
    }

    private async Task<SalesLine> EnsureSalesLineAsync(SalesTable order, string item, decimal qty, decimal price, DateTime date, CancellationToken ct)
    {
        var line = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ItemId == item, ct);
        if (line is null)
        {
            var result = await salesLines.AddAsync(order.RecId, new SalesLineInputDto
            {
                ItemNumber = item, Quantity = qty, UnitPrice = price, Unit = ExampleIdentifiers.Unit, PriceUnit = 1m,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse,
                DeliveryDate = date, TaxGroup = ExampleIdentifiers.CustomerTaxGroup, TaxItemGroup = ExampleIdentifiers.ItemTaxGroup
            }, ct);
            Check(result.Status == SalesLineWriteStatus.Success, result.Error ?? "sales line creation failed.");
            line = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ItemId == item, ct);
        }
        Check(line.SalesQty == qty && line.SalesPrice == price, "existing sales line differs.");
        return line;
    }

    private async Task DeliverAsync(SalesTable order, string key, SalesLine line, decimal qty, DateTime date, CancellationToken ct)
    {
        if (await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ParmId == key, ct)) return;
        var result = await deliveries.PostAsync(order.RecId, new(date, "All", true, key,
            [new PackingSlipPostingLineRequest(line.RecId.ToString(CultureInfo.InvariantCulture), qty)]), ct);
        Success(result.StatusCode, result.Message);
        db.ChangeTracker.Clear();
    }

    private async Task<CustInvoiceJour> InvoiceAsync(SalesTable order, string key, IReadOnlyList<SalesInvoiceLineDto> lines,
        DateTime date, CancellationToken ct)
    {
        if (!await db.Set<CustInvoiceJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ParmId == key, ct))
        {
            var result = await customerInvoices.PostAsync(order.RecId, new(date, key, lines.ToList()), ct);
            Success(result.StatusCode, result.Message);
            db.ChangeTracker.Clear();
        }
        return await db.Set<CustInvoiceJour>().AsNoTracking()
            .SingleAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ParmId == key, ct);
    }

    /// <summary>Post the receipt on account, then settle it against the scenario's invoices, oldest first.</summary>
    private async Task CollectAsync(string customer, string requestId, decimal amount, DateTime date,
        IReadOnlyCollection<string> invoiceIds, CancellationToken ct)
    {
        if (!await db.Set<LedgerJournalTrans>().AnyAsync(x => x.DataAreaId == Area && x.PaymReference == requestId, ct))
        {
            var posted = await custPayments.PostAsync(new(customer, Bank, amount, date, null, null, requestId, SettleAutomatically: false), ct);
            Success(posted.StatusCode, posted.Message);
            db.ChangeTracker.Clear();
        }
        var payment = await db.Set<CustTrans>().AsNoTracking()
            .SingleAsync(x => x.DataAreaId == Area && x.AccountNum == customer && x.PaymReference == requestId, ct);
        var paymentOpen = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == Area && x.RefRecId == payment.RecId)
            .SumAsync(x => (decimal?)x.AmountCur, ct) ?? 0m;
        if (paymentOpen == 0m) return;
        var invoices = await (from open in db.Set<CustTransOpen>()
                              join trans in db.Set<CustTrans>() on open.RefRecId equals trans.RecId
                              where open.DataAreaId == Area && trans.AccountNum == customer && invoiceIds.Contains(trans.Invoice)
                                  && open.AmountCur > 0
                              orderby trans.TransDate, trans.RecId
                              select trans.RecId).ToListAsync(ct);
        var settled = await custSettlements.SettleAsync(new(customer, date, [payment.RecId, .. invoices]), ct);
        Success(settled.StatusCode, settled.Message);
        db.ChangeTracker.Clear();
    }

    private Task<decimal> OpenCustomerAsync(string customer, IReadOnlyCollection<string> invoiceIds, CancellationToken ct) =>
        (from open in db.Set<CustTransOpen>()
         join trans in db.Set<CustTrans>() on open.RefRecId equals trans.RecId
         where open.DataAreaId == Area && trans.AccountNum == customer && invoiceIds.Contains(trans.Invoice)
         select open.AmountCur).SumAsync(ct);

    /// <summary>Debit maintenance expense and credit the accrual liability for 300, posted through the journal services.</summary>
    private async Task<string> AccrueAsync(string key, DateTime date, CancellationToken ct)
    {
        var header = await db.Set<LedgerJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.Name == key, ct)
            ?? await journals.AddAsync(new LedgerJournalTable
            {
                DataAreaId = Area, JournalName = GeneralLedgerExampleSetupSeeder.JournalName, Name = key, CurrencyCode = "SAR"
            }, ct);
        Check(header.JournalName == GeneralLedgerExampleSetupSeeder.JournalName, $"conflicting accrual journal {key}.");
        if (header.Posted != NoYes.Yes)
        {
            if (!await db.Set<LedgerJournalTrans>().AnyAsync(x => x.DataAreaId == Area && x.JournalNum == header.JournalNum, ct))
                await journalLines.AddAsync(new LedgerJournalTrans
                {
                    DataAreaId = Area, JournalNum = header.JournalNum, Voucher = key, TransDate = date,
                    AccountType = LedgerJournalACType.Ledger, LedgerDimension = await DimensionAsync(GeneralLedgerSeedExample.MaintenanceExpense, ct),
                    OffsetAccountType = LedgerJournalACType.Ledger,
                    OffsetLedgerDimension = await DimensionAsync(GeneralLedgerSeedExample.ReceivedNotInvoiced, ct),
                    CurrencyCode = "SAR", AmountCurDebit = 300m, DocumentNum = key, Txt = "GL reference cycle accrued expense"
                }, ct);
            var (posted, error) = await journals.PostAsync(header.RecId, ct);
            if (posted == null) throw new InvalidOperationException($"GL reference cycle: accrual posting failed: {error}");
        }
        db.ChangeTracker.Clear();
        return header.JournalNum;
    }

    private Task<long> DimensionAsync(string code, CancellationToken ct) => db.Set<DimensionAttributeValueCombination>()
        .Where(x => x.DataAreaId == Area && x.MainAccountValue == code).Select(x => x.RecId).SingleAsync(ct);

    /// <summary>Net accounting balance per main account over every voucher this scenario posted.</summary>
    private async Task<Dictionary<string, decimal>> BalancesAsync(VendInvoiceJour purchase, string salesId,
        IReadOnlyCollection<string> payments, string accrualJournal, CancellationToken ct)
    {
        var vouchers = new HashSet<string>(StringComparer.Ordinal);
        vouchers.UnionWith(await db.Set<VendPackingSlipJour>().Where(x => x.DataAreaId == Area && x.PurchId == purchase.PurchId).Select(x => x.LedgerVoucher).ToListAsync(ct));
        vouchers.UnionWith(await db.Set<VendInvoiceJour>().Where(x => x.DataAreaId == Area && x.PurchId == purchase.PurchId).Select(x => x.LedgerVoucher).ToListAsync(ct));
        vouchers.UnionWith(await db.Set<CustPackingSlipJour>().Where(x => x.DataAreaId == Area && x.SalesId == salesId).Select(x => x.LedgerVoucher).ToListAsync(ct));
        vouchers.UnionWith(await db.Set<CustInvoiceJour>().Where(x => x.DataAreaId == Area && x.SalesId == salesId).Select(x => x.LedgerVoucher).ToListAsync(ct));
        vouchers.UnionWith(await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == Area
            && (payments.Contains(x.PaymReference) || x.JournalNum == accrualJournal)).Select(x => x.Voucher).ToListAsync(ct));
        vouchers.RemoveWhere(string.IsNullOrEmpty);
        var entries = await db.Set<SubledgerVoucherGeneralJournalEntry>()
            .Where(x => x.VoucherDataAreaId == Area && vouchers.Contains(x.Voucher)).Select(x => x.GeneralJournalEntry).Distinct().ToListAsync(ct);
        var lines = await (from line in db.Set<GeneralJournalAccountEntry>()
                           join account in db.Set<MainAccount>() on line.MainAccount equals account.RecId
                           where entries.Contains(line.GeneralJournalEntry)
                           select new { account.MainAccountId, line.AccountingCurrencyAmount }).ToListAsync(ct);
        return lines.GroupBy(x => x.MainAccountId).ToDictionary(x => x.Key, x => x.Sum(y => y.AccountingCurrencyAmount), StringComparer.Ordinal);
    }
}
