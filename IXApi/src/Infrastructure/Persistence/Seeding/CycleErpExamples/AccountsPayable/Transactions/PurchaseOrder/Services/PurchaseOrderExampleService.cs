using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal sealed class PurchaseOrderExampleService
{
    internal async Task<(PurchTable Order, PurchLine Line)> SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var supply = new PurchInventorySupplyService(context.Work, context.InventoryNumbers, context.Dimensions);
        var orders = new PurchOrderService(context.Work, context.Actor, context.Actor, context.Numbers,
            supply, new SalesUnitConversionService(context.Work));
        var order = await PurchTableSeeder.SeedAsync(context, orders, ct);
        var line = await PurchLineSeeder.SeedAsync(context, orders, order, ct);
        return (order, line);
    }
}
