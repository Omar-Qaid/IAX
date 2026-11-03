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
        // Materialize the same defaults used when the singleton row is absent.
        // The worker's configured Enabled switch still controls whether it starts.
        if (!await db.Set<BatchSettings>().AnyAsync(x => x.Id == 1, ct))
            db.Add(new BatchSettings { Id = 1, Enabled = true,
                PollIntervalSeconds = Math.Clamp(context.Configuration.GetValue("BackgroundJobs:PollIntervalSeconds", 10), 1, 3600) });
        foreach (var (code, description, concurrency) in new[] { ("DEMO-OPS", "Example operations batch group", 1), ("DEMO-RPT", "Example reporting batch group", 2) })
            if (!await db.Set<SysBackgroundJobGroup>().IgnoreQueryFilters().AnyAsync(x => x.GroupCode == code, ct))
                db.Add(new SysBackgroundJobGroup { GroupCode = code, Description = description,
                    MaxConcurrency = concurrency, SchedulingPriority = 1 });
        foreach (var (code, name, from, to) in new[] { ("DEMO-DAY", "Example daytime window (UTC)", 8 * 3600, 17 * 3600),
            ("DEMO-EVE", "Example evening window (UTC)", 18 * 3600, 23 * 3600) })
            if (!await db.Set<SysBackgroundJobActivePeriod>().IgnoreQueryFilters().AnyAsync(x => x.Code == code, ct))
                db.Add(new SysBackgroundJobActivePeriod { Code = code, Name = name, FromTimeUtc = from, ToTimeUtc = to,
                    FromTimeLocal = from, ToTimeLocal = to, TimeZoneFollowed = 0 });
        // Preserve existing scheduler settings and job enablement.
        await db.SaveChangesAsync(ct);
    }
}
