using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;

internal static class SysBackgroundJobActivePeriodSeeder
{
    internal static Task<SysBackgroundJobActivePeriod> SeedAsync(DbContext db, string code, string name, int from, int to, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.Code == code, SysBackgroundJobActivePeriodDemoData.Create(code, name, from, to), ct);
}
