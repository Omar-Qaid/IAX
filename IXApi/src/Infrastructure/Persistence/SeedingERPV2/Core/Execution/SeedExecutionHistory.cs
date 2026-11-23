using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>Reads and writes <see cref="SysDataSeedLog"/>. Keys stay <c>{id}:v{version}:{scope}</c>, the format earlier runs wrote.</summary>
public sealed class SeedExecutionHistory
{
    public static string Key(string id, int version, string scope) => $"{id}:v{version}:{scope}";

    public Task RecordAsync(ApplicationDbContext db, string id, string company, CancellationToken ct) =>
        RecordAsync(db, id, 1, company, "Repeatable", ct);

    public async Task RecordAsync(ApplicationDbContext db, string id, int version, string scope, string policy,
        CancellationToken ct, long durationMs = 0)
    {
        var key = Key(id, version, scope);
        if (!await db.Set<SysDataSeedLog>().AnyAsync(x => x.TableName == key && x.Outcome == "Success", ct))
            db.Set<SysDataSeedLog>().Add(new SysDataSeedLog
            {
                TableName = key, SeederId = id, Scope = scope, Version = version,
                Policy = policy, Outcome = "Success", DurationMs = durationMs
            });
        await db.SaveChangesAsync(ct);
    }

    public async Task RecordFailureAsync(ApplicationDbContext db, string id, int version, string scope, string policy,
        Exception error, long durationMs)
    {
        db.ChangeTracker.Clear();
        db.Set<SysDataSeedLog>().Add(new SysDataSeedLog
        {
            TableName = Key(id, version, scope), SeederId = id, Scope = scope, Version = version,
            Policy = policy, Outcome = "Failed", DurationMs = durationMs, Error = error.ToString()
        });
        await db.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>True when a run-once seeder already succeeded, including rows written before SeederId was recorded.</summary>
    public Task<bool> CompletedAsync(ApplicationDbContext db, string id, int version, string scope, CancellationToken ct)
    {
        var key = Key(id, version, scope);
        return db.Set<SysDataSeedLog>().AsNoTracking().AnyAsync(x => x.Outcome == "Success"
            && ((x.SeederId == id && x.Version == version && x.Scope == scope)
                || (x.SeederId == null && (x.TableName == id || x.TableName == key))), ct);
    }
}
