using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace IAX.IXApi.Infrastructure.Persistence.Services
{
    public abstract class BaseService<T> : IBaseService<T> where T : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ICurrentUserService _currentUser;
        protected readonly IGenericRepository<T> _repository;

        protected BaseService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _repository = _unitOfWork.Repository<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.GetQueryable().AsNoTracking();
            query = ApplyDomainFilters(query);
            if (include != null) query = include(query);
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(string[] includes, CancellationToken cancellationToken = default)
        {
            var query = _repository.GetQueryable().AsNoTracking();
            query = ApplyDomainFilters(query);
            if (includes != null)
            {
                foreach (var includePath in includes)
                {
                    if (!string.IsNullOrWhiteSpace(includePath))
                        query = query.Include(includePath);
                }
            }
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<PagedResultDto<T>> GetPagedAsync(QueryFilterDto paginationParams, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.GetQueryable().AsNoTracking();
            query = ApplyDomainFilters(query);
            
            if (include != null) query = include(query);
            
            if (paginationParams.Includes != null && paginationParams.Includes.Any())
            {
                foreach (var includePath in paginationParams.Includes)
                {
                    if (!string.IsNullOrWhiteSpace(includePath))
                        query = query.Include(includePath);
                }
            }

            query = query.WhereDataGrid(paginationParams);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                             .Take(paginationParams.PageSize)
                             .ToListAsync(cancellationToken);

            return new PagedResultDto<T>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(object id, string[] includes, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, includes, cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(object id, Func<IQueryable<T>, IIncludableQueryable<T, object>> include, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, include, cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = asNoTracking ? _repository.GetQueryable().AsNoTracking() : _repository.GetQueryable();
            query = ApplyDomainFilters(query);
            if (include != null) query = include(query);
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(Expression<Func<T, bool>> predicate, string[]? includes = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = asNoTracking ? _repository.GetQueryable().AsNoTracking() : _repository.GetQueryable();
            query = ApplyDomainFilters(query);
            if (includes != null)
            {
                foreach (var include in includes) query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T?> GetByCompositeKeyAsync(object?[]? keyFields, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByCompositeKeyAsync(keyFields, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = _repository.GetQueryable();
            query = ApplyDomainFilters(query);
            if (asNoTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await OnBeforeAddAsync(entity, cancellationToken);
            var result = await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return result;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            foreach (var e in entities) await OnBeforeAddAsync(e, cancellationToken);
            var result = await _repository.AddRangeAsync(entities, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return result;
        }

        /// <summary>
        /// Hook invoked before an entity is added. Override to generate codes, set defaults, etc.
        /// </summary>
        protected virtual Task OnBeforeAddAsync(T entity, CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var result = await _repository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return result;
        }

        public virtual async Task<T> UpdateWithDetailsAsync<TDetail>(T entity, Expression<Func<T, IEnumerable<TDetail>>> detailSelector, CancellationToken cancellationToken = default) where TDetail : class
        {
             return await UpdateAsync(entity, cancellationToken);
        }

        public virtual async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _repository.RemoveAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            var result = await _repository.UpdateRangeAsync(entities);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return result;
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _repository.RemoveRangeAsync(entities);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        public virtual async Task<T?> GetFirstOrDefaultAsync(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.GetEntityWithSpec(spec, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetListAsync(IAX.IXApi.Shared.Domain.Entities.ISpecification<T> spec, CancellationToken cancellationToken = default)
        {
            return await _repository.ListWithSpec(spec, cancellationToken);
        }

        /// <summary>
        /// Hook to apply domain-specific filters (e.g., multi-tenancy, soft delete).
        /// </summary>
        protected virtual IQueryable<T> ApplyDomainFilters(IQueryable<T> query)
        {
            return query;
        }
    }
}
