using System.Linq.Expressions;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

/// <summary>Company-scoped, insert-or-validate access. Existing setup is never overwritten.</summary>
public sealed class SeedContext(ApplicationDbContext db, int fiscalYear, string actor, string? company = null)
{
    public ApplicationDbContext Db { get; } = db;
    /// <summary>The company being seeded: the explicit one (startup has no request company) or the current one.</summary>
    public string Company => company ?? Db.GetDataAreaId();
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
        CheckLengths(proposed);
        Db.Set<T>().Add(proposed);
        await Db.SaveChangesAsync(ct);
        return proposed;
    }

    /// <summary>
    /// Baseline insert: adds <paramref name="proposed"/> in <paramref name="area"/> (default <see cref="Company"/>) when
    /// no row matches <paramref name="key"/>, and otherwise leaves the existing row exactly as the operator keeps it.
    /// </summary>
    /// <remarks>Works for any key type: the company and audit columns are addressed by name, as every entity maps them.</remarks>
    public async Task<T> AddMissingAsync<T>(Expression<Func<T, bool>> key, T proposed, CancellationToken ct, string? area = null)
        where T : class
    {
        area ??= Company;
        var existing = await Db.Set<T>().IgnoreQueryFilters()
            .Where(x => EF.Property<string>(x, nameof(BaseEntity<long>.DataAreaId)) == area).FirstOrDefaultAsync(key, ct);
        if (existing != null) return existing;
        var entry = Db.Entry(proposed);
        entry.Property(nameof(BaseEntity<long>.DataAreaId)).CurrentValue = area;
        entry.Property(nameof(BaseEntity<long>.CreatedBy)).CurrentValue = Actor;
        entry.Property(nameof(BaseEntity<long>.OwnerAccountId)).CurrentValue = Actor;
        CheckLengths(proposed);
        Db.Set<T>().Add(proposed);
        await Db.SaveChangesAsync(ct);
        return proposed;
    }

    /// <summary><see cref="AddMissingAsync{T}"/> for each row, matched on the key the selector builds.</summary>
    public async Task AddMissingAsync<T>(IEnumerable<T> rows, Func<T, Expression<Func<T, bool>>> key, CancellationToken ct,
        string? area = null) where T : class
    {
        foreach (var row in rows) await AddMissingAsync(key(row), row, ct, area);
    }

    private void CheckLengths<T>(T proposed) where T : class
    {
        foreach (var property in Db.Model.FindEntityType(typeof(T))!.GetProperties())
            if (property.PropertyInfo?.GetValue(proposed) is string value && property.GetMaxLength() is int max && value.Length > max)
                throw new InvalidOperationException($"{typeof(T).Name}.{property.Name} exceeds its mapped length {max}.");
    }

    private bool IsActive<T>(T row) where T : BaseEntity<long>
    {
        var value = Db.Entry(row).Property("IsActive").CurrentValue;
        return value is bool active ? active : value != null && Convert.ToInt32(value) == 1;
    }
}
