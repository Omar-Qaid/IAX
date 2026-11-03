using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

public sealed class PurchaseCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var db = context.Db;
        var supply = new PurchInventorySupplyService(context.Work, context.InventoryNumbers, context.Dimensions);
        var orders = new PurchOrderService(context.Work, context.Actor, context.Actor, context.Numbers,
            supply, new SalesUnitConversionService(context.Work));
        var order = await db.Set<PurchTable>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.PurchName == Purchase && x.ReturnItemNum == "", ct);
        if (order == null)
        {
            var result = await orders.CreateAsync(new PurchOrderCreateDto(Vendor, null, Currency, context.Date, Site, Warehouse, Purchase), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            order = (PurchTable)result.Data!;
        }
        if (order.IsDeleted || order.OrderAccount != Vendor || order.CurrencyCode != Currency)
            throw new InvalidOperationException("ERP purchase example conflicts with an existing order; it was preserved.");
        var line = await db.Set<PurchLine>().SingleOrDefaultAsync(x => x.PurchId == order.PurchId, ct);
        if (line == null)
        {
            var result = await orders.AddLineAsync(order.RecId, new PurchLineInputDto(Item, PurchaseQuantity, Unit,
                PurchasePrice, 1m, Site, Warehouse, context.Date, "ERP cycle: office chairs for resale"), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == order.PurchId, ct);
        }
        if (line.ItemId != Item || line.QtyOrdered != PurchaseQuantity)
            throw new InvalidOperationException("ERP purchase example lines were customized; no further posting was attempted.");
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.PurchId == order.PurchId && x.PackingSlipId == Purchase + "-R", ct))
        {
            var confirmed = await new PurchConfirmationService(context.Work, context.Actor, context.Actor, context.Numbers)
                .ConfirmAsync(order.RecId, ct);
            ErpCycleExampleContext.Require(confirmed.StatusCode, confirmed.Message);
            var received = await new ProductReceiptService(context.Work, context.Actor, context.Actor, context.Numbers)
                .PostAsync(order.RecId, new ProductReceiptPostDto(Purchase + "-R", context.Date,
                    new() { new PurchPostLineDto(line.RecId, PurchaseQuantity) }), ct);
            ErpCycleExampleContext.Require(received.StatusCode, received.Message);
        }
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.PurchId == order.PurchId && x.InvoiceId == Purchase + "-I", ct))
        {
            var result = await new VendorInvoiceService(context.Work, context.Actor, context.Actor, context.Numbers)
                .PostAsync(order.RecId, new VendorInvoicePostDto(Purchase + "-I", context.Date,
                    new() { new PurchPostLineDto(line.RecId, PurchaseQuantity) }), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
        }
    }
}
