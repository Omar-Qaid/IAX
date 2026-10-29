using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>Resumable two-item, picking, reconfirmation, discount and alternate-unit posting examples.</summary>
public sealed class SalesAdvancedScenarioSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.SalesAdvancedScenarios";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.InventoryCompletionScenarios" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var actor = new SalesScenarioSeeder.ExampleActor(await context.Db.Set<AspNetUser>().IgnoreQueryFilters()
            .Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct));
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(context.Db.Database.GetConnectionString()).Options, companyContext: actor);
        var work = new UnitOfWork(db);
        var numbers = new SysNumberSequenceService(work, actor);
        var inventNumbers = new SalesInventoryNumberService(numbers);
        var resolver = new InventDimensionResolver(work, inventNumbers);
        var units = new SalesUnitConversionService(work);
        var date = DateTime.UtcNow.Date;

        // Four boxes supply twenty reams, using the existing item-specific 5:1 conversion.
        const string stockMarker = "DEMO-SALES-UOM-STOCK";
        var purch = new PurchOrderService(work, actor, actor, numbers, new PurchInventorySupplyService(work, inventNumbers, resolver), units);
        var po = await db.Set<PurchTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.PurchName == stockMarker, ct);
        if (po == null)
        {
            var created = await purch.CreateAsync(new PurchOrderCreateDto("VEND-100", null, "SAR", date, "JED", "JED-1", stockMarker), ct);
            Check(created.StatusCode, created.Message);
            po = (PurchTable)created.Data!;
        }
        var pl = await db.Set<PurchLine>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct);
        if (pl == null)
        {
            var added = await purch.AddLineAsync(po.RecId, new PurchLineInputDto("ITM-1002", 4m, "BOX", 100m, 1m, "JED", "JED-1", date, "Example paper supply"), ct);
            Check(added.StatusCode, added.Message);
            pl = await db.Set<PurchLine>().SingleAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct);
        }
        if (pl.QtyOrdered != 20m) throw new InvalidOperationException("Example supply requires four BOX to equal twenty PCK; customized units were preserved.");
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct))
        {
            var confirmed = await new PurchConfirmationService(work, actor, actor, numbers).ConfirmAsync(po.RecId, ct);
            Check(confirmed.StatusCode, confirmed.Message);
            var receipt = await new ProductReceiptService(work, actor, actor, numbers).PostAsync(po.RecId,
                new ProductReceiptPostDto(stockMarker + "-R", date, new() { new PurchPostLineDto(pl.RecId, 4m) }), ct);
            Check(receipt.StatusCode, receipt.Message);
        }
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == "dat" && x.PurchId == po.PurchId, ct))
        {
            var invoice = await new VendorInvoiceService(work, actor, actor, numbers).PostAsync(po.RecId,
                new VendorInvoicePostDto(stockMarker + "-I", date, new() { new PurchPostLineDto(pl.RecId, 4m) }), ct);
            Check(invoice.StatusCode, invoice.Message);
        }

        var validation = new SalesOrderValidationService(work, actor);
        var demand = new SalesInventoryDemandService(work, inventNumbers, resolver);
        var orders = new SalesOrderQuickCreateService(work, actor, numbers, validation);
        var lines = new SalesOrderLineService(work, actor, validation, demand, units);
        var confirmation = new SalesOrderConfirmationService(work, actor, numbers, actor, new SalesOrderConfirmationValidator());
        var reservations = new InventoryReservationService(work, actor);
        var delivery = new PackingSlipJournalService(work, actor, actor, numbers);
        var invoicing = new SalesInvoiceService(work, actor, actor, numbers);
        foreach (var marker in new[] { "DEMO-SO-TWOITEM", "DEMO-SO-DISCOUNT-UOM" })
        {
            var discount = marker.EndsWith("UOM", StringComparison.Ordinal);
            var order = await db.Set<SalesTable>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.CustomerRef == marker, ct);
            if (order == null)
            {
                var created = await orders.CreateAsync(new SalesOrderQuickCreateDto { CustomerAccount = "CUST-100", CustomerReference = marker,
                    SalesName = marker, CurrencyCode = "SAR", InventSiteId = "JED", InventLocationId = "JED-1", TaxGroupId = "DOM",
                    RequestedReceiptDate = date, RequestedShipDate = date }, ct);
                if (created.Error != null) throw new InvalidOperationException(created.Error);
                order = await db.Set<SalesTable>().SingleAsync(x => x.DataAreaId == "dat" && x.CustomerRef == marker, ct);
            }
            var posted = await db.Set<CustInvoiceJour>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct);
            if (posted != null)
            {
                ValidateTotals(posted, discount);
                continue;
            }
            foreach (var item in new[] { "ITEM-001", "ITM-1002" })
            {
                if (await db.Set<SalesLine>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId && x.ItemId == item, ct)) continue;
                var paper = item == "ITM-1002";
                var input = Input(item, paper ? (discount ? 1m : 5m) : (discount ? 2m : 9m), date, discount);
                var added = await lines.AddAsync(order.RecId, input, ct);
                if (added.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(added.Error);
            }
            var firstConfirmation = await confirmation.PostAsync(order.RecId, new PostConfirmationRequest(date, true, marker + "-C1"), ct);
            Check(firstConfirmation.StatusCode, firstConfirmation.Message);
            var selected = await db.Set<SalesLine>().Where(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId).OrderBy(x => x.LineNum).ToListAsync(ct);
            if (!discount)
            {
                var chair = selected.Single(x => x.ItemId == "ITEM-001");
                if (chair.SalesQty == 9m)
                {
                    var changed = await lines.ChangeAsync(order.RecId, chair.RecId, Input("ITEM-001", 10m, date, false), ct);
                    if (changed.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(changed.Error);
                }
                var reconfirmed = await confirmation.PostAsync(order.RecId, new PostConfirmationRequest(date, true, marker + "-C2"), ct);
                Check(reconfirmed.StatusCode, reconfirmed.Message);
            }
            if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct))
            {
                foreach (var line in selected)
                {
                    if (line.ItemId == "ITM-1002" && line.QtyOrdered != 5m)
                        throw new InvalidOperationException("Paper example must issue five inventory PCK.");
                    // Reset only the pick target; unpicking restores reservation, not physical stock.
                    var reset = await reservations.SetPickedAsync(line.InventTransId, 0m, ct);
                    if (reset.Error != null) throw new InvalidOperationException(reset.Error);
                    var reserved = await reservations.ReserveAsync(line.InventTransId, line.QtyOrdered, false, ct);
                    if (reserved.Error != null) throw new InvalidOperationException(reserved.Error);
                    foreach (var target in new[] { line.QtyOrdered / 2m, 0m, line.QtyOrdered, line.QtyOrdered })
                    {
                        var picked = await reservations.SetPickedAsync(line.InventTransId, target, ct);
                        if (picked.Error != null) throw new InvalidOperationException(picked.Error);
                    }
                }
                var shipped = await delivery.PostAsync(order.RecId, new PostPackingSlipRequest(date, "Picked", true, marker + "-PS",
                    selected.Select(x => new PackingSlipPostingLineRequest(x.RecId.ToString(), x.SalesQty)).ToArray()), ct);
                Check(shipped.StatusCode, shipped.Message);
            }
            var invoiced = await invoicing.PostAsync(order.RecId, new SalesInvoicePostDto(date, marker + "-INV"), ct);
            Check(invoiced.StatusCode, invoiced.Message);
            ValidateTotals(await db.Set<CustInvoiceJour>().SingleAsync(x => x.DataAreaId == "dat" && x.SalesId == order.SalesId, ct), discount);
        }
    }

    private static SalesLineInputDto Input(string item, decimal qty, DateTime date, bool discount) => new() {
        ItemNumber = item, Quantity = qty, Unit = item == "ITEM-001" ? "PCS" : discount ? "BOX" : "PCK",
        UnitPrice = item == "ITEM-001" ? 100m : discount ? 1000m : 200m, PriceUnit = 1m,
        InventSiteId = "JED", InventLocationId = "JED-1", TaxGroup = "DOM", TaxItemGroup = "FULL", DeliveryDate = date,
        LineDiscountPercent = discount ? 10m : 0m, MultiLineDiscount = discount && item == "ITEM-001" ? 10m : 0m
    };

    private static void ValidateTotals(CustInvoiceJour invoice, bool discount)
    {
        var net = discount ? 1070m : 2000m;
        if (invoice.SalesBalance != net || invoice.SumTax != net * .15m || invoice.InvoiceAmount != net * 1.15m)
            throw new InvalidOperationException("Sales example totals differ from its expected prices, discounts and 15% tax; posted records were preserved.");
    }
    private static void Check(int status, string? message)
    {
        if (status is < 200 or >= 300) throw new InvalidOperationException(message);
    }
}
