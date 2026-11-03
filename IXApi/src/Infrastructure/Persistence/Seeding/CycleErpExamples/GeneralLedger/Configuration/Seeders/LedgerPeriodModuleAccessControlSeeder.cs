using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerPeriodModuleAccessControlSeeder
{
    internal static Task<LedgerPeriodModuleAccessControl> SeedAsync(DbContext db, long period, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.LedgerFiscalCalendarPeriod == period, LedgerPeriodModuleAccessControlDemoData.Create(period), ct);
}
