using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
internal static class FoundationLookup
{
    public static async Task EnsureAsync<T>(SeedContext c, string code, string name, CancellationToken ct) where T : class, new()
    {
        var existing = await c.Db.Set<T>().IgnoreQueryFilters().SingleOrDefaultAsync(x => EF.Property<string>(x, "DataAreaId") == c.Company && EF.Property<string>(x, "Code") == code, ct);
        if (existing != null)
        {
            var entry = c.Db.Entry(existing);
            if (!Equals(entry.Property("Name").CurrentValue, name) || !Equals(entry.Property("IsActive").CurrentValue, true) || Equals(entry.Property("IsDeleted").CurrentValue, true))
                throw new InvalidOperationException($"Conflicting Foundation lookup {typeof(T).Name} in {c.Company}.");
            return;
        }
        var row = new T();
        var added = c.Db.Entry(row);
        foreach (var pair in new Dictionary<string, object> { ["DataAreaId"] = c.Company, ["Code"] = code, ["Name"] = name, ["CreatedBy"] = c.Actor, ["OwnerAccountId"] = c.Actor })
            added.Property(pair.Key).CurrentValue = pair.Value;
        c.Db.Add(row);
        await c.Db.SaveChangesAsync(ct);
    }
}
