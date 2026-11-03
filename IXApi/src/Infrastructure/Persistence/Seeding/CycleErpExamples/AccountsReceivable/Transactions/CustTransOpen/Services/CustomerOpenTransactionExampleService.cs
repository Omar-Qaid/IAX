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

internal sealed class CustomerOpenTransactionExampleService
{
    internal async Task RequireAsync(ErpCycleExampleContext context, CustTrans transaction, CancellationToken ct)
    {
        var remaining = CustTransOpenDemoData.Remaining(transaction);
        var row = await context.Db.Set<CustTransOpen>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.RefRecId == transaction.RecId, ct);
        if (remaining == 0m && row == null) return;
        if (row == null || row.IsDeleted || row.AccountNum != transaction.AccountNum || row.AmountCur != remaining)
            throw new InvalidOperationException("The ERP customer open transaction conflicts with the posted balance; it was preserved.");
    }
}
