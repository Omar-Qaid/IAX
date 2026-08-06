using System.Linq.Expressions;

namespace IAX.IXApi.Infrastructure.Persistence.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(object id, string[] includes, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(object id, Func<IQueryable<T>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, object>> include, CancellationToken cancellationToken = default);
        
        Task<T?> GetByCompositeKeyAsync(object?[]? keyFields, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity);
        Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);
        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entities);
        IQueryable<T> GetQueryable();
        Task<T?> GetEntityWithSpec(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> ListWithSpec(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<int> CountAsync(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default);
    }
}
