using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal sealed class VendorSettlementExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, VendInvoiceJour invoice, CancellationToken ct)
    {
        var db = context.Db;
        var previous = await db.Set<LedgerJournalTrans>().SingleOrDefaultAsync(x => x.PaymReference == VendorPayment, ct);
        var paid = await new VendPaymentService(context.Work, context.Actor, context.Actor, context.Numbers).PostAsync(
            VendSettlementDemoData.Create(invoice, previous?.TransDate ?? context.Date), ct);
        ErpCycleExampleContext.Require(paid.StatusCode, paid.Message);
    }
}
