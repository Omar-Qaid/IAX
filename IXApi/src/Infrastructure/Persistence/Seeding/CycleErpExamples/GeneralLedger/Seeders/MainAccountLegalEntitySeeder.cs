using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class MainAccountLegalEntitySeeder
{
    internal static Task<MainAccountLegalEntity> SeedAsync(DbContext db, long account, long company, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.MainAccount == account && x.LegalEntity == company, MainAccountLegalEntityDemoData.Create(account, company), ct);
}
