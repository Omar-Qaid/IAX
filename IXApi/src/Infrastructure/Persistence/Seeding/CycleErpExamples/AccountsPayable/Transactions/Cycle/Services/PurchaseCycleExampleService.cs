using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

public sealed class PurchaseCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var (order, line) = await new PurchaseOrderExampleService().SeedAsync(context, ct);
        await VendPurchOrderJourSeeder.SeedAsync(context, order, line, ct);
        await VendPackingSlipJourSeeder.SeedAsync(context, order, line, ct);
        var invoice = await VendInvoiceJourSeeder.SeedAsync(context, order, line, ct);
        await VendSettlementSeeder.SeedAsync(context, invoice, ct);
    }

    public Task EnsureConfirmationAsync(DbContext db, PurchTable order, PurchLine line,
        Func<CancellationToken, Task<PurchResult>> confirm, CancellationToken ct = default, bool allowCreate = true) =>
        new PurchaseConfirmationExampleService().EnsureConfirmationAsync(db, order, line, confirm, ct, allowCreate);
}
