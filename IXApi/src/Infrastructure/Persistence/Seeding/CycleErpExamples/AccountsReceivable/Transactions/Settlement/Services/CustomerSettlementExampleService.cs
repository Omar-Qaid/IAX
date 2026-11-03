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

internal sealed class CustomerSettlementExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CustInvoiceJour invoice, CancellationToken ct)
    {
        var db = context.Db;
        var previousPayment = await db.Set<LedgerJournalTrans>().SingleOrDefaultAsync(x => x.PaymReference == Payment, ct);
        var paid = await new CustPaymentService(context.Work, context.Actor, context.Actor, context.Numbers).PostAsync(
            CustSettlementDemoData.Create(invoice, previousPayment?.TransDate ?? context.Date), ct);
        ErpCycleExampleContext.Require(paid.StatusCode, paid.Message);
    }
}
