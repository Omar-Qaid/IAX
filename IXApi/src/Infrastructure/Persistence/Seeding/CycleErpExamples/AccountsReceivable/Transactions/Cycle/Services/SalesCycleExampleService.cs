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

public sealed class SalesCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var (order, line) = await new SalesOrderExampleService().SeedAsync(context, ct);
        await CustConfirmJourSeeder.SeedAsync(context, order, ct);
        await CustPackingSlipJourSeeder.SeedAsync(context, order, line, ct);
        var invoice = await CustInvoiceJourSeeder.SeedAsync(context, order, line, ct);
        var transaction = await CustTransSeeder.SeedAsync(context, invoice, ct);
        await CustTransOpenSeeder.SeedAsync(context, transaction, ct);
        await CustSettlementSeeder.SeedAsync(context, invoice, ct);
    }
}
