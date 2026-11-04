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

    public async Task<T> RequireAsync<T>(Expression<Func<T, bool>> key, CancellationToken ct)
        where T : BaseEntity<long>
    {
        var matches = await Rows<T>().Where(key).Take(2).ToListAsync(ct);
        if (matches.Count != 1)
            throw new InvalidOperationException($"Expected exactly one {typeof(T).Name} reference in {Company}; found {matches.Count}.");
        var row = matches[0];
        if (row.IsDeleted || !IsActive(row))
            throw new InvalidOperationException($"Required {typeof(T).Name} is deleted or inactive in {Company}.");
        return row;
    }

    public async Task<T> EnsureAsync<T>(Expression<Func<T, bool>> key, T proposed,
        CancellationToken ct, params string[] fields) where T : BaseEntity<long>
    {
        var existing = await Rows<T>().SingleOrDefaultAsync(key, ct);
        if (existing != null)
        {
            if (existing.IsDeleted || IsActive(existing) != IsActive(proposed))
                throw new InvalidOperationException($"{typeof(T).Name} seed key is deleted or inactive in {Company}.");
            if (fields.Length == 0)
                fields = Db.Model.FindEntityType(typeof(T))!.GetProperties()
                    .Where(p => p.PropertyInfo?.DeclaringType == typeof(T) && !p.IsPrimaryKey()
                        && p.Name != "RowVersion" && p.Name != "IsActive")
                    .Select(p => p.Name).ToArray();
            foreach (var field in fields)
            {
                var property = Db.Model.FindEntityType(typeof(T))!.FindProperty(field)?.PropertyInfo
                    ?? throw new InvalidOperationException($"Unknown seed field {typeof(T).Name}.{field}.");
                if (!Equals(property.GetValue(existing), property.GetValue(proposed)))
                    throw new InvalidOperationException($"Conflicting {typeof(T).Name}.{field} in {Company}; existing data was preserved.");
            }
            return existing;
        }
        proposed.DataAreaId = Company;
        proposed.CreatedBy = Actor;
        proposed.OwnerAccountId = Actor;
        foreach (var property in Db.Model.FindEntityType(typeof(T))!.GetProperties())
            if (property.PropertyInfo?.GetValue(proposed) is string value && property.GetMaxLength() is int max && value.Length > max)
                throw new InvalidOperationException($"{typeof(T).Name}.{property.Name} exceeds its mapped length {max}.");
        Db.Set<T>().Add(proposed);
        await Db.SaveChangesAsync(ct);
        return proposed;
    }

    private bool IsActive<T>(T row) where T : BaseEntity<long>
    {
        var value = Db.Entry(row).Property("IsActive").CurrentValue;
        return value is bool active ? active : value != null && Convert.ToInt32(value) == 1;
    }
}
