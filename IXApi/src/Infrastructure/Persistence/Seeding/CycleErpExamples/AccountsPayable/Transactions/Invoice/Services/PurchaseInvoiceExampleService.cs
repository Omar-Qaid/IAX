using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal sealed class PurchaseInvoiceExampleService
{
    internal async Task<VendInvoiceJour> SeedAsync(ErpCycleExampleContext context, PurchTable order, PurchLine line, CancellationToken ct)
    {
        var db = context.Db;
        if (!await db.Set<VendInvoiceJour>().AnyAsync(x => x.PurchId == order.PurchId && x.InvoiceId == Purchase + "-I", ct))
        {
            var result = await new VendorInvoiceService(context.Work, context.Actor, context.Actor, context.Numbers)
                .PostAsync(order.RecId, VendInvoiceJourDemoData.Create(context.Date, line.RecId), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
        }
        var invoice = await db.Set<VendInvoiceJour>().SingleAsync(x => x.PurchId == order.PurchId && x.InvoiceId == Purchase + "-I", ct);
        return invoice;
    }
}
