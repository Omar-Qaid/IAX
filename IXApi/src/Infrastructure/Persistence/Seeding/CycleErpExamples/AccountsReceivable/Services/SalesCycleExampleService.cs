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
        var order = await SalesTableSeeder.SeedAsync(context, orders, ct);
        var line = await SalesLineSeeder.SeedAsync(context, lines, order, ct);
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
