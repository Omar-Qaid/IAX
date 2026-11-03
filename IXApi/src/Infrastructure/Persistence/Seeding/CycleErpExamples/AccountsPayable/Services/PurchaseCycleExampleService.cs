using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
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
            var result = await orders.CreateAsync(new PurchOrderCreateDto(Vendor, null, CurrencyCode, context.Date, Site, Warehouse, Purchase), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            order = (PurchTable)result.Data!;
        }
        if (order.IsDeleted || order.OrderAccount != Vendor || order.InvoiceAccount != Vendor || order.CurrencyCode != CurrencyCode
            || order.InventSiteId != Site || order.InventLocationId != Warehouse || order.PurchaseType != PurchaseType.Purch)
            throw new InvalidOperationException("ERP purchase example conflicts with an existing order; it was preserved.");
        var line = await db.Set<PurchLine>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.PurchId == order.PurchId, ct);
        if (line == null)
        {
            var result = await orders.AddLineAsync(order.RecId, new PurchLineInputDto(Item, PurchaseQuantity, Unit,
                PurchasePrice, 1m, Site, Warehouse, context.Date, "ERP cycle: office chairs for resale"), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == order.PurchId, ct);
        }
        if (line.IsDeleted || line.ItemId != Item || line.QtyOrdered != PurchaseQuantity || line.PurchQty != PurchaseQuantity
            || line.PurchPrice != PurchasePrice || line.PurchUnit != Unit || line.PriceUnit != 1m
            || line.CurrencyCode != CurrencyCode || line.VendAccount != Vendor || line.LineAmount != PurchaseQuantity * PurchasePrice)
            throw new InvalidOperationException("ERP purchase example lines were customized; no further posting was attempted.");
        await ErpCycleStorageIntegrity.RequireAsync(db, line.InventDimId, ct);
        var hasReceipt = await db.Set<VendPackingSlipJour>().IgnoreQueryFilters()
            .AnyAsync(x => x.DataAreaId == Company && x.PurchId == order.PurchId, ct);
        await EnsureConfirmationAsync(db, order, line,
            token => new PurchConfirmationService(context.Work, context.Actor, context.Actor, context.Numbers)
                .ConfirmAsync(order.RecId, token), ct, allowCreate: !hasReceipt);
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.PurchId == order.PurchId && x.PackingSlipId == Purchase + "-R", ct))
        {
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
        var invoice = await db.Set<VendInvoiceJour>().SingleAsync(x => x.PurchId == order.PurchId && x.InvoiceId == Purchase + "-I", ct);
        var previous = await db.Set<LedgerJournalTrans>().SingleOrDefaultAsync(x => x.PaymReference == VendorPayment, ct);
        var paid = await new VendPaymentService(context.Work, context.Actor, context.Actor, context.Numbers).PostAsync(
            new VendPaymentPostDto(Vendor, "DEMO-SAR", invoice.InvoiceAmount, VendorPayment,
                previous?.TransDate ?? context.Date, VendorPaymentMode, invoice.InvoiceId), ct);
        ErpCycleExampleContext.Require(paid.StatusCode, paid.Message);
    }

    public async Task EnsureConfirmationAsync(DbContext db, PurchTable order, PurchLine line,
        Func<CancellationToken, Task<PurchResult>> confirm, CancellationToken ct = default, bool allowCreate = true)
    {
        if (order.DataAreaId != Company || line.DataAreaId != Company || order.PurchId != line.PurchId)
            throw new InvalidOperationException("ERP purchase confirmation references a different order or company.");
        var existing = await db.Set<VendPurchOrderJour>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.PurchId == order.PurchId, ct);
        if (existing == null)
        {
            if (!allowCreate)
                throw new InvalidOperationException("A received ERP purchase example has no confirmation snapshot; historical data was preserved.");
            var result = await confirm(ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            return;
        }
        if (existing.IsDeleted || existing.OrderAccount != order.OrderAccount || existing.InvoiceAccount != order.InvoiceAccount
            || existing.CurrencyCode != order.CurrencyCode || existing.Qty != line.PurchQty || existing.Amount != line.LineAmount)
            throw new InvalidOperationException("ERP purchase confirmation was customized; the existing snapshot was preserved.");
        var snapshot = await db.Set<VendPurchOrderTrans>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.PurchId == order.PurchId && x.PurchOrderDocNum == existing.PurchOrderDocNum, ct);
        if (snapshot == null || snapshot.IsDeleted || snapshot.ItemId != line.ItemId || snapshot.Qty != line.PurchQty
            || snapshot.PurchUnit != line.PurchUnit || snapshot.PurchPrice != line.PurchPrice || snapshot.PriceUnit != line.PriceUnit
            || snapshot.LineAmount != line.LineAmount || snapshot.InventTransId != line.InventTransId || snapshot.InventDimId != line.InventDimId)
            throw new InvalidOperationException("ERP purchase confirmation lines conflict with the example; no replacement was created.");
    }
}
