using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;

internal static class SysBackgroundJobGroupSeeder
{
    internal static Task<SysBackgroundJobGroup> SeedAsync(DbContext db, string code, string description, int concurrency, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.GroupCode == code, SysBackgroundJobGroupDemoData.Create(code, description, concurrency), ct);
}
