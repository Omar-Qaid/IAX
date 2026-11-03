using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

public sealed class FoundationCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var db = context.Db;
        // Reuse the reference/master seeders. Never duplicate customers, vendors, products or ledger setup here.
        _ = await db.Set<CompanyInfo>().SingleAsync(x => x.DataArea == ErpCycleExampleData.Company, ct);
        _ = await db.Set<CustTable>().SingleAsync(x => x.AccountNum == ErpCycleExampleData.Customer, ct);
        _ = await db.Set<VendTable>().SingleAsync(x => x.AccountNum == ErpCycleExampleData.Vendor, ct);
        _ = await db.Set<InventTable>().SingleAsync(x => x.ItemId == ErpCycleExampleData.Item, ct);
        var ledger = await db.Set<Ledger>().SingleAsync(ct);
        if (ledger.AccountingCurrency != ErpCycleExampleData.Currency || ledger.ReportingCurrency != ErpCycleExampleData.Currency)
            throw new InvalidOperationException("The ERP cycle requires the existing SAR example ledger; customized currencies were preserved.");
    }
}
