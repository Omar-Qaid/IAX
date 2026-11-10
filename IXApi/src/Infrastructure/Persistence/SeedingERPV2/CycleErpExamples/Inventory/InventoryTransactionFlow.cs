using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

/// <summary>
/// The executable Inventory scenarios. Only owning business services create documents, stock movements or
/// accounting entries; this class sequences them and then hands the posted state to the reconciliation checks.
/// Document numbers are derived from a stable scenario key, so a run interrupted halfway resumes rather than
/// posting a second cycle.
/// </summary>
public sealed partial class InventoryTransactionFlow(ApplicationDbContext db, IPurchOrderService purchases,
    IPurchConfirmationService confirmations, IProductReceiptService receipts, IVendorInvoiceService vendorInvoices,
    ISalesOrderQuickCreateService sales, ISalesOrderLineService salesLines, IPackingSlipJournalService deliveries,
    ISalesInvoiceService customerInvoices, IInventoryReservationService reservations, IInventCloseService closing,
    IInventJournalService journals, IInventJournalPostingService journalPosting,
    IAX.IXApi.Modules.Finance.Inventory.Transfer.IInventTransferOrderService transfers)
{
    /// <summary>
    /// The scenarios this package can execute. A reference case is listed here only once it runs end to end
    /// through real services: an id present here is a claim that its figures were asserted against posted rows.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Scenarios { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["01"] = "Purchase, reserve, partial delivery and invoice, service line, second purchase, weighted-average close",
        ["02"] = "Positive and negative inventory adjustment, profit/loss accounting and safe replay",
        ["03"] = "Counting shortage from 90 to 88, loss accounting and safe replay",
        ["04"] = "Counting overage from 90 to 93, profit accounting and safe replay",
        ["05"] = "Matching count of 90, no variance posting and safe replay",
        ["06"] = "Transfer order shipment, partial receipt, transit balance and final receipt",
        ["07"] = "Same-site transfer journal conserving quantity and value",
        ["08"] = "Movement journal receipt and issue with explicit offset account",
        ["09"] = "Weighted-average recalculation and subsequent posting within its cutoff"
    };

    private string Area => db.GetDataAreaId();
    private readonly List<InventoryStageReport> stages = [];

    private static void Check(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException("Inventory cycle integrity: " + message);
    }

    private static void Success(int status, string? error) => Check(status is 200 or 201, error ?? $"Posting failed ({status}).");

    public async Task<InventoryScenarioReport> ExecuteAsync(InventoryTransactionOptions options, string id, CancellationToken ct)
    {
        stages.Clear();
        if (!Scenarios.ContainsKey(id)) throw new ArgumentException($"Inventory scenario {id} is not implemented.", nameof(id));
        if (id == "02") return await AdjustmentCycleAsync(options, ct);
        if (id is "03" or "04" or "05") return await CountingCycleAsync(options, id, ct);
        if (id == "06") return await TransferCycleAsync(options, ct);
        if (id == "07") return await TransferJournalCycleAsync(options, ct);
        if (id == "08") return await MovementCycleAsync(options, ct);
        if (id == "09") return await RecalculationCycleAsync(options, ct);
        var item = ExampleIdentifiers.StockItem(options.FiscalYear);
        var service = ExampleIdentifiers.ServiceItem(options.FiscalYear);
        var key = ExampleIdentifiers.ScenarioKey(options.FiscalYear, id);
        var date = new DateTime(options.FiscalYear, 11, 1);
        var closeDate = date.AddDays(29);

        // A completed close is the scenario's terminal state. Reconciling it again is the rerun behavior; posting
        // a second cycle onto the same item would invalidate the period average every figure below depends on.
        var alreadyClosed = await db.Set<InventClosing>().AnyAsync(x => x.DataAreaId == Area && x.ScopeItemId == item
            && x.TransDate == closeDate && x.Cancellation == NoYes.No && x.CancelClosing == NoYes.No
            && x.RunRecalculation == NoYes.No, ct);
        if (!alreadyClosed) await ReferenceCycleAsync(options, key, item, service, date, closeDate, ct);

        return await ReconcileAsync(id, item, service, 90m, 1080m, 720m, -120m, 0m, ct);
    }

    /// <summary>
    /// The eleven reference steps. Of the 100 bought at 10, 60 are reserved and then sold in two deliveries; a
    /// second receipt of 50 at 16 arrives after both sales, which lifts the period average to 12 and so adjusts
    /// the already-invoiced issues upward at close.
    /// </summary>
    private async Task ReferenceCycleAsync(InventoryTransactionOptions options, string key, string item,
        string service, DateTime date, DateTime closeDate, CancellationToken ct)
    {
        await BuyAsync(key + "-P1", item, 100m, 10m, date, ct);

        var customer = ExampleIdentifiers.Customer(Area);
        var order = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.SalesName == key, ct);
        if (order is null)
        {
            var created = await sales.CreateAsync(new SalesOrderQuickCreateDto
            {
                CustomerAccount = customer, CurrencyCode = "SAR", SalesName = key,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse,
                RequestedShipDate = date.AddDays(4), RequestedReceiptDate = date.AddDays(4)
            }, ct);
            Check(created.Error is null && created.Order is not null, created.Error ?? "Sales order creation failed.");
            order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == Area && x.RecId == created.Order!.RecId, ct);
        }
        Check(order.CustAccount == customer && order.CurrencyCode == "SAR", "Existing sales order differs.");

        var stockLine = await EnsureSalesLineAsync(order, item, 60m, 25m, date, ct);
        var serviceLine = await EnsureSalesLineAsync(order, service, 1m, 300m, date, ct);

        if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId, ct))
        {
            var reserved = await reservations.ReserveAsync(stockLine.InventTransId, 60m, false, ct);
            Check(reserved.Error is null, reserved.Error ?? "Reservation failed.");
            var sum = await db.Set<InventSum>().SingleAsync(x => x.DataAreaId == Area && x.ItemId == item, ct);
            Check(sum.PhysicalInvent == 100m && sum.ReservPhysical == 60m && sum.AvailPhysical == 40m,
                "Reservation quantities differ.");
            stages.Add(new("RES-01", "Reserve 60 of 100", order.SalesId, 60m, sum.ReservPhysical, null, null, [], "Passed"));
        }

        await DeliverAsync(order, key + "-D1", stockLine, 40m, date.AddDays(5), ct);
        await InvoiceAsync(order, key + "-I1", [new(stockLine.RecId, 40m)], date.AddDays(6), ct);
        await DeliverAsync(order, key + "-D2", stockLine, 20m, date.AddDays(7), ct);
        await InvoiceAsync(order, key + "-I2", [new(stockLine.RecId, 20m), new(serviceLine.RecId, 1m)], date.AddDays(8), ct);

        // 60 pieces at 25 plus the 300 service line. The service is billed only because it is non-stocked, so this
        // total is what proves the non-stocked line reached invoicing rather than being silently dropped.
        var invoices = await db.Set<CustInvoiceJour>()
            .Where(x => x.DataAreaId == Area && (x.ParmId == key + "-I1" || x.ParmId == key + "-I2")).ToListAsync(ct);
        Check(invoices.Count == 2 && invoices.Sum(x => x.InvoiceAmount) == 1800m,
            "Expected two sales invoices totaling 1,800.");
        stages.Add(new("ISS-03", "Stock and service invoicing", key, null, null, 1800m,
            invoices.Sum(x => x.InvoiceAmount), invoices.Select(x => x.LedgerVoucher).ToArray(), "Passed"));

        await BuyAsync(key + "-P2", item, 50m, 16m, date.AddDays(9), ct);

        // Before the close the issues still carry the running cost of 10, so inventory stands at 1,200 rather
        // than the 1,080 the period average produces. Asserting this separates a costing fault from a close fault.
        var before = await db.Set<InventTrans>().Where(x => x.DataAreaId == Area && x.ItemId == item).ToListAsync(ct);
        Check(before.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) == 1200m,
            "Pre-close inventory value must be 1,200 at the running cost.");
        stages.Add(new("CST-01", "Pre-close running cost", item, 90m, before.Where(x =>
            x.StatusReceipt is StatusReceipt.Received or StatusReceipt.Purchased
            || x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold).Sum(x => x.Qty),
            1200m, before.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment), [], "Passed"));

        var result = await closing.CloseItemAsync(item, closeDate, ct);
        Check(result.Error is null, result.Error ?? "Close failed.");
        stages.Add(new("CST-04", "Weighted-average close", item, null, null, 1080m, null, [], "Posted"));
    }

    private async Task<InventoryScenarioReport> ReconcileAsync(string id, string item, string service,
        decimal quantity, decimal value, decimal cost, decimal adjustment, decimal reserved, CancellationToken ct)
    {
        var posted = await InventoryReconciliationSnapshot.LoadAsync(db, item, service, id,
            quantity, value, cost, adjustment, reserved, ct);
        foreach (var check in InventoryReconciliationSnapshot.Checks) check.Verify(posted);
        return new(id, Scenarios[id], item, stages.ToArray(), posted.PhysicalQuantity, posted.InventoryValue,
            posted.CostOfGoodsSold, posted.CloseAdjustment, posted.ReservedQuantity,
            posted.PostedVouchers.Count, posted.Settlements.Count);
    }

    /// <summary>Purchase, confirm, receive and invoice one quantity at one price, through the AP services.</summary>
    private async Task BuyAsync(string key, string item, decimal qty, decimal price, DateTime date, CancellationToken ct)
    {
        var account = ExampleIdentifiers.Vendor(Area);
        var order = await db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.PurchName == key, ct);
        if (order is null)
        {
            var created = await purchases.CreateAsync(new(account, null, "SAR", date,
                ExampleIdentifiers.Site, ExampleIdentifiers.Warehouse, key, date), ct);
            Success(created.StatusCode, created.Message);
            order = (PurchTable)created.Data!;
        }
        Check(order.OrderAccount == account && order.CurrencyCode == "SAR", "Existing purchase order differs.");

        var line = await db.Set<PurchLine>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct);
        if (line is null)
        {
            var added = await purchases.AddLineAsync(order.RecId,
                new(item, qty, ExampleIdentifiers.Unit, price, 1m, null, null, date, item), ct);
            Success(added.StatusCode, added.Message);
            line = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct);
        }
        Check(line.ItemId == item && line.PurchQty == qty && line.PurchPrice == price, "Existing purchase line differs.");

        if (!await db.Set<VendPurchOrderJour>().AnyAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId, ct))
        {
            var result = await confirmations.ConfirmAsync(order.RecId, ct, date);
            Success(result.StatusCode, result.Message);
        }
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.PurchId == order.PurchId && x.PackingSlipId == key + "-R", ct))
        {
            var result = await receipts.PostAsync(order.RecId, new(key + "-R", date.AddDays(1), [new(line.RecId, qty)]), ct);
            Success(result.StatusCode, result.Message);
        }
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == Area && x.InvoiceAccount == account && x.InvoiceId == key + "-I", ct))
        {
            var result = await vendorInvoices.PostAsync(order.RecId, new(key + "-I", date.AddDays(2), [new(line.RecId, qty)]), ct);
            Success(result.StatusCode, result.Message);
        }
        stages.Add(new("RCV-01", $"Purchase {qty} at {price.ToString(CultureInfo.InvariantCulture)}", key, qty, qty, null, null, [], "Posted"));
    }

    private async Task<SalesLine> EnsureSalesLineAsync(SalesTable order, string item, decimal qty, decimal price,
        DateTime date, CancellationToken ct)
    {
        var line = await db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ItemId == item, ct);
        if (line is null)
        {
            var result = await salesLines.AddAsync(order.RecId, new SalesLineInputDto
            {
                ItemNumber = item, Quantity = qty, UnitPrice = price, Unit = ExampleIdentifiers.Unit, PriceUnit = 1m,
                InventSiteId = ExampleIdentifiers.Site, InventLocationId = ExampleIdentifiers.Warehouse,
                DeliveryDate = date, TaxGroup = ExampleIdentifiers.CustomerTaxGroup,
                TaxItemGroup = ExampleIdentifiers.ItemTaxGroup
            }, ct);
            Check(result.Status == SalesLineWriteStatus.Success, result.Error ?? "Sales line creation failed.");
            line = await db.Set<SalesLine>().SingleAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ItemId == item, ct);
        }
        Check(line.SalesQty == qty && line.SalesPrice == price, "Existing sales line differs.");
        return line;
    }

    /// <summary>
    /// Posts a packing slip for an explicit quantity. The "All" selection with a stated quantity is used instead
    /// of "Picked", which would filter to registered picking lines this cycle never creates.
    /// </summary>
    private async Task DeliverAsync(SalesTable order, string key, SalesLine line, decimal qty, DateTime date, CancellationToken ct)
    {
        if (await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ParmId == key, ct)) return;
        var result = await deliveries.PostAsync(order.RecId, new(date, "All", true, key,
            [new PackingSlipPostingLineRequest(line.RecId.ToString(CultureInfo.InvariantCulture), qty)]), ct);
        Success(result.StatusCode, result.Message);
        stages.Add(new("ISS-01", $"Deliver {qty}", key, qty, qty, null, null, [], "Posted"));
    }

    private async Task InvoiceAsync(SalesTable order, string key, IReadOnlyList<SalesInvoiceLineDto> lines,
        DateTime date, CancellationToken ct)
    {
        if (await db.Set<CustInvoiceJour>().AnyAsync(x => x.DataAreaId == Area && x.SalesId == order.SalesId && x.ParmId == key, ct)) return;
        var result = await customerInvoices.PostAsync(order.RecId, new(date, key, lines.ToList()), ct);
        Success(result.StatusCode, result.Message);
    }
}
