using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendInvoiceJourSeeder
{
    internal static Task<VendInvoiceJour> SeedAsync(ErpCycleExampleContext context, PurchTable order, PurchLine line, CancellationToken ct) =>
        new PurchaseInvoiceExampleService().SeedAsync(context, order, line, ct);
}
