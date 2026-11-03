using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

// Called inside the existing module seeder transaction; never replaces existing rows.
internal static class ExampleSeedStore
{
    internal static async Task<T> EnsureAsync<T>(DbContext db, Expression<Func<T, bool>> key, T row, CancellationToken ct) where T : class
    {
        var existing = await db.Set<T>().IgnoreQueryFilters().SingleOrDefaultAsync(key, ct);
        if (existing != null)
        {
            if (db.Entry(existing).Metadata.FindProperty("IsDeleted") != null &&
                db.Entry(existing).Property("IsDeleted").CurrentValue is true)
                throw new InvalidOperationException($"An example {typeof(T).Name} key belongs to a deleted record; it was preserved.");
            return existing;
        }
        db.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }
}
