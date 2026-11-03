using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;

internal static class BatchSettingsSeeder
{
    internal static Task<BatchSettings> SeedAsync(DbContext db, int pollInterval, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.Id == 1, BatchSettingsDemoData.Create(pollInterval), ct);
}
