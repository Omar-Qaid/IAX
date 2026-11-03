using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal sealed class PurchaseReceiptExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, PurchTable order, PurchLine line, CancellationToken ct)
    {
        var db = context.Db;
        if (!await db.Set<VendPackingSlipJour>().AnyAsync(x => x.PurchId == order.PurchId && x.PackingSlipId == Purchase + "-R", ct))
        {
            var received = await new ProductReceiptService(context.Work, context.Actor, context.Actor, context.Numbers)
                .PostAsync(order.RecId, VendPackingSlipJourDemoData.Create(context.Date, line.RecId), ct);
            ErpCycleExampleContext.Require(received.StatusCode, received.Message);
        }
    }
}
