using System.Linq.Expressions;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

/// <summary>Company-scoped, insert-or-validate access. Existing setup is never overwritten.</summary>
public sealed class SeedContext(ApplicationDbContext db, int fiscalYear, string actor)
{
    public ApplicationDbContext Db { get; } = db;
    public string Company => Db.GetDataAreaId();
    public int FiscalYear { get; } = fiscalYear;
    public string Actor { get; } = actor;

    public IQueryable<T> Rows<T>() where T : BaseEntity<long> => Db.Set<T>().IgnoreQueryFilters()
        .Where(x => x.DataAreaId == Company);

    public Task<T> RequireAsync<T>(Expression<Func<T, bool>> key, CancellationToken ct)
        where T : BaseEntity<long> => Rows<T>().Where(x => !x.IsDeleted && x.IsActive).SingleAsync(key, ct);

    public async Task<T> EnsureAsync<T>(Expression<Func<T, bool>> key, T proposed,
        CancellationToken ct, params string[] fields) where T : BaseEntity<long>
    {
        var existing = await Rows<T>().SingleOrDefaultAsync(key, ct);
        if (existing != null)
        {
            if (existing.IsDeleted || !((BaseEntity<long>)existing).IsActive)
                throw new InvalidOperationException($"{typeof(T).Name} seed key is deleted or inactive in {Company}.");
            foreach (var field in fields)
            {
                var property = typeof(T).GetProperty(field)
                    ?? throw new InvalidOperationException($"Unknown seed field {typeof(T).Name}.{field}.");
                if (!Equals(property.GetValue(existing), property.GetValue(proposed)))
                    throw new InvalidOperationException($"Conflicting {typeof(T).Name}.{field} in {Company}; existing data was preserved.");
            }
            return existing;
        }
        proposed.DataAreaId = Company;
        proposed.CreatedBy = Actor;
        proposed.OwnerAccountId = Actor;
        Db.Set<T>().Add(proposed);
        await Db.SaveChangesAsync(ct);
        return proposed;
    }
}
