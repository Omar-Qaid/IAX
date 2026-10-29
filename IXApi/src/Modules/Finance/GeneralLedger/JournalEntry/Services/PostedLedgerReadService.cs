using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

/// <summary>Generic access to posted accounting is read-only. Posting services own its creation.</summary>
public abstract class PostedLedgerReadService<T>(IUnitOfWork work, ICurrentUserService user)
    : BaseService<T>(work, user) where T : class
{
    private static NotSupportedException ReadOnly() =>
        new("Posted accounting cannot be changed through generic services. Use the supported posting or reversal process.");

    public sealed override Task<T> AddAsync(T entity, CancellationToken cancellationToken = default) => throw ReadOnly();
    public sealed override Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) => throw ReadOnly();
    public sealed override Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default) => throw ReadOnly();
    public sealed override Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) => throw ReadOnly();
    public sealed override Task RemoveAsync(T entity, CancellationToken cancellationToken = default) => throw ReadOnly();
    public sealed override Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) => throw ReadOnly();
    public sealed override Task<T> UpdateWithDetailsAsync<TDetail>(T entity,
        Expression<Func<T, IEnumerable<TDetail>>> detailSelector, CancellationToken cancellationToken = default) => throw ReadOnly();
}
