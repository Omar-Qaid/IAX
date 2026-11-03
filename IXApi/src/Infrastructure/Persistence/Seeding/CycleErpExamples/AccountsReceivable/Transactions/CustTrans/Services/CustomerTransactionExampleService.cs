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

internal sealed class CustomerTransactionExampleService
{
    internal async Task<CustTrans> RequireAsync(ErpCycleExampleContext context, CustInvoiceJour invoice, CancellationToken ct)
    {
        var expected = CustTransDemoData.Expected(invoice);
        var row = await context.Db.Set<CustTrans>().IgnoreQueryFilters().SingleAsync(x =>
            x.DataAreaId == Company && x.Invoice == invoice.InvoiceId && x.AccountNum == Customer, ct);
        if (row.IsDeleted || row.AccountNum != expected.Account || row.CurrencyCode != expected.Currency || row.AmountCur != expected.Amount)
            throw new InvalidOperationException("The ERP customer transaction conflicts with its invoice; it was preserved.");
        return row;
    }
}
