using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public interface ISysNumberSequenceService : IBaseService<SysNumberSequence>
    {
        /// <summary>
        /// Atomically reserves and returns the next sequence value + formatted code for the given entity.
        /// Thread-safe — guards against duplicate/skipped numbers under concurrent load.
        /// </summary>
        Task<NextSequenceResultDto> NextAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the next code WITHOUT consuming the value (for preview/UI).
        /// </summary>
        Task<NextSequenceResultDto?> PeekAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets a sequence to its smallest value, or to <paramref name="nextValue"/> when provided.
        /// </summary>
        Task<SysNumberSequence> ResetAsync(int id, long? nextValue, CancellationToken cancellationToken = default);
    }
}
