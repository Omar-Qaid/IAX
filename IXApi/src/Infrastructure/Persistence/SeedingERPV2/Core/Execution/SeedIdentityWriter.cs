using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>
/// Saves reference rows that carry their own key although the column is an identity column.
/// </summary>
/// <remarks>
/// Most seed data lets the database assign the key. A few reference tables cannot: process
/// definitions and saved form controls store the key of the category, control or performer they point
/// at, so those keys are part of the data and a reseed has to reproduce them exactly. SQL Server
/// refuses an explicit identity value unless the session allows it for that table, while the SQLite
/// test provider accepts one, so the statements are issued only where they are needed. The runner
/// already owns an open connection and a transaction; this joins them rather than opening its own.
/// </remarks>
public static class SeedIdentityWriter
{
    public static async Task SaveAsync<T>(SeedContext context, CancellationToken ct) where T : class
    {
        var table = context.Db.Model.FindEntityType(typeof(T))?.GetTableName()
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not mapped to a table.");
        if (!context.Db.Database.IsSqlServer())
        {
            await context.Db.SaveChangesAsync(ct);
            return;
        }
        await context.Db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{table}] ON", ct);
        try
        {
            await context.Db.SaveChangesAsync(ct);
        }
        finally
        {
            await context.Db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{table}] OFF", CancellationToken.None);
        }
    }
}
