using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal sealed class VendorConfigurationExampleService
{
    internal async Task SeedAsync(DbContext db, CancellationToken ct)
    {
        if (!await db.Set<VendLedgerAccounts>().IgnoreQueryFilters().AnyAsync(x =>
                x.DataAreaId == "dat" && x.PostingProfile == "DEMO" && !x.IsDeleted, ct))
            throw new InvalidOperationException("The example vendor posting profile DEMO is missing.");
        await VendParametersSeeder.SeedAsync(db, ct);
        await VendPaymModeTableSeeder.SeedAsync(db, "ELECTRONIC", "Electronic vendor payment", ct);
        await VendPaymModeTableSeeder.SeedAsync(db, "CHECK", "Vendor check payment", ct);
    }
}
