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

internal sealed class SalesOrderExampleService
{
    internal async Task<(SalesTable Order, SalesLine Line)> SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var validation = new SalesOrderValidationService(context.Work, context.Actor);
        var demand = new SalesInventoryDemandService(context.Work, context.InventoryNumbers, context.Dimensions);
        var orders = new SalesOrderQuickCreateService(context.Work, context.Actor, context.Numbers, validation);
        var lines = new SalesOrderLineService(context.Work, context.Actor, validation, demand, new SalesUnitConversionService(context.Work));
        var order = await SalesTableSeeder.SeedAsync(context, orders, ct);
        var line = await SalesLineSeeder.SeedAsync(context, lines, order, ct);
        return (order, line);
    }
}
