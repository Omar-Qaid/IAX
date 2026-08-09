using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IAX.IXApi.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    internal DbSet<T> dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        this.dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, (string[]?)null, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(object id, string[]? includes, CancellationToken cancellationToken = default)
    {
        var entityType = _context.Model.FindEntityType(typeof(T));
        var primaryKey = entityType?.FindPrimaryKey();
        var keyProperty = primaryKey?.Properties[0];
        var keyType = keyProperty?.ClrType;

        if (keyType != null && id is string strId)
        {
            var underlyingType = Nullable.GetUnderlyingType(keyType) ?? keyType;
            try
            {
                id = Convert.ChangeType(strId, underlyingType);
            }
            catch
            {
                // Primary key is numeric (long/int), but id string is a business code (e.g., "DHL", "FOB", "NET30").
                // Look up by matching string code property or return null.
                var codeProp = FindBusinessCodeProperty(entityType);
                if (codeProp != null)
                {
                    var query = dbSet.AsQueryable();
                    if (includes != null)
                    {
                        foreach (var include in includes) query = query.Include(include);
                    }
                    return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, codeProp.Name) == strId, cancellationToken);
                }
                return null;
            }
        }

        if (includes != null && includes.Any())
        {
            var query = dbSet.AsQueryable();
            foreach (var include in includes) query = query.Include(include);
            
            var keyName = keyProperty?.Name ?? "Id";
            return await query.FirstOrDefaultAsync(e => EF.Property<object>(e, keyName).Equals(id), cancellationToken);
        }

        return await dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(object id, Func<IQueryable<T>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, object>> include, CancellationToken cancellationToken = default)
    {
        var entityType = _context.Model.FindEntityType(typeof(T));
        var primaryKey = entityType?.FindPrimaryKey();
        var keyProperty = primaryKey?.Properties[0];
        var keyType = keyProperty?.ClrType;

        if (keyType != null && id is string strId)
        {
            var underlyingType = Nullable.GetUnderlyingType(keyType) ?? keyType;
            try
            {
                id = Convert.ChangeType(strId, underlyingType);
            }
            catch
            {
                // Primary key is numeric but id string is a business code (e.g., "DHL", "FOB").
                // Look up by matching string code property.
                var codeProp = FindBusinessCodeProperty(entityType);
                if (codeProp != null)
                {
                    var query = dbSet.AsQueryable();
                    if (include != null) query = include(query);
                    return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, codeProp.Name) == strId, cancellationToken);
                }
                return null;
            }
        }

        var query2 = dbSet.AsQueryable();
        if (include != null) query2 = include(query2);

        var keyName = keyProperty?.Name ?? "Id";
        return await query2.FirstOrDefaultAsync(e => EF.Property<object>(e, keyName).Equals(id), cancellationToken);
    }

    public async Task<T?> GetByCompositeKeyAsync(object?[]? keyFields, CancellationToken cancellationToken = default)
    {
        return await dbSet.FindAsync(keyFields, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public Task<T> UpdateAsync(T entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            dbSet.Attach(entity);
            entry.State = EntityState.Modified;
        }
        return Task.FromResult(entity);
    }
   
    public Task RemoveAsync(T entity)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            dbSet.Attach(entity);
        }
        dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                dbSet.Attach(entity);
                entry.State = EntityState.Modified;
            }
        }
        return Task.FromResult(entities);
    }

    public Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
        }
        dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public IQueryable<T> GetQueryable()
    {
        return dbSet.AsQueryable();
    }

    public async Task<T?> GetEntityWithSpec(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> ListWithSpec(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).CountAsync(cancellationToken);
    }

    private IQueryable<T> ApplySpecification(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(dbSet.AsQueryable(), spec);
    }

    /// <summary>
    /// Finds the best string property to use as a business code lookup when the
    /// primary key is numeric but the caller supplied a string identifier.
    /// Priority: "Code" > "{EntityName}Id" > first [Required] string ending in Code/Id.
    /// </summary>
    private static Microsoft.EntityFrameworkCore.Metadata.IProperty? FindBusinessCodeProperty(
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType)
    {
        if (entityType == null) return null;

        var stringProps = entityType.GetProperties()
            .Where(p => p.ClrType == typeof(string))
            .ToList();

        var entityName = typeof(T).Name; // e.g. "DlvMode", "PaymTerm", "MarkupTable"

        // 1. Exact match: "Code"
        var match = stringProps.FirstOrDefault(p => p.Name == "Code");
        if (match != null) return match;

        // 2. Convention: "{EntityName}Id" (e.g. PaymTermId for PaymTerm)
        match = stringProps.FirstOrDefault(p => p.Name == entityName + "Id");
        if (match != null) return match;

        // 3. Convention: exact entity name (e.g. PaymSched property on PaymSched entity)
        match = stringProps.FirstOrDefault(p => p.Name == entityName);
        if (match != null) return match;

        // 4. First required string property whose name ends in "Code" or "Id"
        //    (e.g. MarkupCode on MarkupTable)
        match = stringProps.FirstOrDefault(p =>
            p.PropertyInfo?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), true).Length > 0
            && (p.Name.EndsWith("Code") || p.Name.EndsWith("Id")));
        if (match != null) return match;

        return null;
    }
}
