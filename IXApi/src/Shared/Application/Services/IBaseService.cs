using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Infrastructure.Persistence.Services;

public interface IBaseService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken cancellationToken = default);

    /// <summary>Fetch all records eager-loading the given navigation paths (e.g. for DTO flattening).</summary>
    Task<IEnumerable<T>> GetAllAsync(string[] includes, CancellationToken cancellationToken = default);

    Task<PagedResultDto<T>> GetPagedAsync(QueryFilterDto paginationParams, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken cancellationToken = default);
    
    // Core overloads for GetByIdAsync to avoid ambiguity
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(object id, string[] includes, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(object id, Func<IQueryable<T>, IIncludableQueryable<T, object>> include, bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    // Predicate overloads
    Task<T?> GetByIdAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(Expression<Func<T, bool>> predicate, string[]? includes = null, bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    Task<T?> GetByCompositeKeyAsync(object?[]? keyFields, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    Task<T> UpdateWithDetailsAsync<TDetail>(T entity, Expression<Func<T, IEnumerable<TDetail>>> detailSelector, CancellationToken cancellationToken = default) where TDetail : class;
    
    Task RemoveAsync(T entity, CancellationToken cancellationToken = default);
    
    Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    Task<T?> GetFirstOrDefaultAsync(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetListAsync(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default);
}
