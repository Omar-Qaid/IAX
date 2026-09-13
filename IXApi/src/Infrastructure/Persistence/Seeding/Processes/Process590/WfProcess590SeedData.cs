using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        await SeedProcessesAsync(db, owner, ct);
        await SeedStepsAsync(db, owner, ct);
        await SeedActivitiesAsync(db, owner, ct);
        await SeedActivityControlsAsync(db, owner, ct);
        await SeedRequestControlsAsync(db, owner, ct);
        await SeedTransitionsAsync(db, owner, ct);
        await SeedVariablesAsync(db, owner, ct);
        await SeedUsersProcessesAsync(db, owner, ct);
        await SeedOptionsAndValidationsAsync(db, owner, ct);
        await SeedRequestsAsync(db, owner, ct);
        await SeedRequestMappingVariablesAsync(db, owner, ct);
        await SeedActivityMappingVariablesAsync(db, owner, ct);
        await SeedPrintTemplateAsync(db, owner, ct);
        // Add Performers seeding here when available
    }

    private static async Task SaveWithIdentityAsync(ApplicationDbContext db, string table, CancellationToken ct)
    {
        if (!db.ChangeTracker.HasChanges()) return;

        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, $"SET IDENTITY_INSERT {table} ON", ct);
            await db.SaveChangesAsync(ct);
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, $"SET IDENTITY_INSERT {table} OFF", ct);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
