using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>Reuse existing seed history, without adding another persistence table.</summary>
public sealed class SeedExecutionHistory
{
    public async Task RecordAsync(ApplicationDbContext db, string id, string company, CancellationToken ct)
    {
        var key = $"{id}:v1:{company}";
        var log = await db.Set<SysDataSeedLog>().SingleOrDefaultAsync(x => x.TableName == key, ct);
        if (log == null)
            db.Set<SysDataSeedLog>().Add(new SysDataSeedLog
            {
                TableName = key, SeederId = id, Scope = company, Version = 1,
                Policy = "Repeatable", Outcome = "Success"
            });
        await db.SaveChangesAsync(ct);
    }
}
