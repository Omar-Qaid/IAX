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

internal sealed class SalesPackingSlipExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, SalesTable order, SalesLine line, CancellationToken ct)
    {
        var db = context.Db;
        if (!await db.Set<CustPackingSlipJour>().AnyAsync(x => x.SalesId == order.SalesId && x.ParmId == Sales, ct))
        {
            var result = await new PackingSlipJournalService(context.Work, context.Actor, context.Actor, context.Numbers)
                .PostAsync(order.RecId, CustPackingSlipJourDemoData.Create(context.Date, line.RecId), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
        }
    }
}
