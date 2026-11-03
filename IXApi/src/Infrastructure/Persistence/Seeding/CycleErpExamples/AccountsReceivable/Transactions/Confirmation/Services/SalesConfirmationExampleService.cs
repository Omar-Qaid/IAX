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

internal sealed class SalesConfirmationExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, SalesTable order, CancellationToken ct)
    {
        var db = context.Db;
        if (!await db.Set<CustConfirmJour>().AnyAsync(x => x.SalesId == order.SalesId && x.ParmId == Sales, ct))
        {
            var result = await new SalesOrderConfirmationService(context.Work, context.Actor, context.Numbers,
                context.Actor, new SalesOrderConfirmationValidator()).PostAsync(order.RecId,
                    CustConfirmJourDemoData.Create(context.Date), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
        }
    }
}
