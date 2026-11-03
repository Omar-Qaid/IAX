using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;

public sealed class SalesCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var db = context.Db;
        var validation = new SalesOrderValidationService(context.Work, context.Actor);
        var demand = new SalesInventoryDemandService(context.Work, context.InventoryNumbers, context.Dimensions);
        var orders = new SalesOrderQuickCreateService(context.Work, context.Actor, context.Numbers, validation);
        var lines = new SalesOrderLineService(context.Work, context.Actor, validation, demand, new SalesUnitConversionService(context.Work));
        var order = await db.Set<SalesTable>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.CustomerRef == Sales, ct);
        if (order == null)
        {
            var result = await orders.CreateAsync(new SalesOrderQuickCreateDto { CustomerAccount = Customer,
                CustomerReference = Sales, SalesName = Sales, CurrencyCode = CurrencyCode,
                InventSiteId = Site, InventLocationId = Warehouse, RequestedReceiptDate = context.Date,
                RequestedShipDate = context.Date, TaxGroupId = "DOM" }, ct);
            if (result.Error != null) throw new InvalidOperationException(result.Error);
            order = await db.Set<SalesTable>().SingleAsync(x => x.CustomerRef == Sales, ct);
        }
        if (order.IsDeleted || order.CustAccount != Customer || order.InvoiceAccount != Customer || order.CurrencyCode != CurrencyCode
            || order.InventSiteId != Site || order.InventLocationId != Warehouse || order.ReturnItemNum != "")
            throw new InvalidOperationException("ERP sales example conflicts with an existing order; it was preserved.");
        var line = await db.Set<SalesLine>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.SalesId == order.SalesId, ct);
        if (line == null)
        {
            var result = await lines.AddAsync(order.RecId, new SalesLineInputDto { ItemNumber = Item,
                Quantity = SalesQuantity, Unit = Unit, UnitPrice = SalesPrice, PriceUnit = 1m,
                InventSiteId = Site, InventLocationId = Warehouse, Description = "ERP cycle: office chairs sold",
                DeliveryDate = context.Date, TaxGroup = "DOM" }, ct);
            if (result.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(result.Error);
            line = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == order.SalesId, ct);
        }
        if (line.IsDeleted || line.ItemId != Item || line.SalesQty != SalesQuantity || line.QtyOrdered != SalesQuantity
            || line.SalesPrice != SalesPrice || line.SalesUnit != Unit || line.PriceUnit != 1m || line.CustAccount != Customer)
            throw new InvalidOperationException("ERP sales example lines were customized; no further posting was attempted.");
        await ErpCycleStorageIntegrity.RequireAsync(db, line.InventDimId, ct);
        if (!await db.Set<CustConfirmJour>().AnyAsync(x => x.SalesId == order.SalesId && x.ParmId == Sales, ct))
        {
            var result = await new SalesOrderConfirmationService(context.Work, context.Actor, context.Numbers,
                context.Actor, new SalesOrderConfirmationValidator()).PostAsync(order.RecId,
                    new PostConfirmationRequest(context.Date, true, Sales), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
        }
        if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.SalesId == order.SalesId && x.ParmId == Sales, ct))
        {
            var result = await new PackingSlipJournalService(context.Work, context.Actor, context.Actor, context.Numbers)
                .PostAsync(order.RecId, new PostPackingSlipRequest(context.Date, "All", true, Sales,
                    new[] { new PackingSlipPostingLineRequest(line.RecId.ToString(), SalesQuantity) }), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
        }
        if (!await db.Set<CustInvoiceJour>().AnyAsync(x => x.SalesId == order.SalesId && x.ParmId == Sales, ct))
        {
            var result = await new SalesInvoiceService(context.Work, context.Actor, context.Actor, context.Numbers)
                .PostAsync(order.RecId, new SalesInvoicePostDto(context.Date, Sales,
                    new() { new SalesInvoiceLineDto(line.RecId, SalesQuantity) }), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
        }
        var invoice = await db.Set<CustInvoiceJour>().SingleAsync(x => x.SalesId == order.SalesId && x.ParmId == Sales, ct);
        var previousPayment = await db.Set<LedgerJournalTrans>().SingleOrDefaultAsync(x => x.PaymReference == Payment, ct);
        var paid = await new CustPaymentService(context.Work, context.Actor, context.Actor, context.Numbers).PostAsync(
            new CustPaymentPostDto(Customer, "DEMO-SAR", invoice.InvoiceAmount,
                previousPayment?.TransDate ?? context.Date, "Transfer", invoice.InvoiceId, Payment), ct);
        ErpCycleExampleContext.Require(paid.StatusCode, paid.Message);
    }
}
