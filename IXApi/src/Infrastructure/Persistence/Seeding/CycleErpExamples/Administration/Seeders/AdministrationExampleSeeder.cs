using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed class AdministrationExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Administration.BatchExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override IReadOnlyCollection<string> DependsOn => new[] { "Administration.Settings" };
    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        await BatchSettingsSeeder.SeedAsync(db, context.Configuration.GetValue("BackgroundJobs:PollIntervalSeconds", 10), ct);
        foreach (var (code, description, concurrency) in new[] { ("DEMO-OPS", "Example operations batch group", 1), ("DEMO-RPT", "Example reporting batch group", 2) })
            await SysBackgroundJobGroupSeeder.SeedAsync(db, code, description, concurrency, ct);
        foreach (var (code, name, from, to) in new[] { ("DEMO-DAY", "Example daytime window (UTC)", 8 * 3600, 17 * 3600),
            ("DEMO-EVE", "Example evening window (UTC)", 18 * 3600, 23 * 3600) })
            await SysBackgroundJobActivePeriodSeeder.SeedAsync(db, code, name, from, to, ct);
        // Preserve existing scheduler settings and job enablement.
        await db.SaveChangesAsync(ct);
    }
}
