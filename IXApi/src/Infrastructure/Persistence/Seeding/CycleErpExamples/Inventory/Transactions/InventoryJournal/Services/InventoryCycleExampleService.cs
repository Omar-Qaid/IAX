using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

public sealed class InventoryCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        // The purchase and sale already generated receipt/issue inventory movements through posting services.
        // Add a separate, unposted adjustment for a user to review; never close unrelated company inventory.
        var db = context.Db;
        var journal = await InventJournalTableSeeder.SeedAsync(context, ct);
        await InventJournalTransSeeder.SeedAsync(context, journal, ct);
    }
}
