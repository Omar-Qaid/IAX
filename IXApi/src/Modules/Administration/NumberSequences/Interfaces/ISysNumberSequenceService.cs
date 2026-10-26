using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.NumberSequences;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public interface ISysNumberSequenceService : IBaseService<SysNumberSequence>, INumberSequenceRuntime
    {
        Task<string> GetNextSequenceAsync(string entityName, string? company = null, CancellationToken cancellationToken = default);
        Task<NextSequenceResultDto> NextAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default);
        Task<NextSequenceResultDto?> PeekAsync(string entityName, string? tenantId = null, CancellationToken cancellationToken = default);
        Task<SysNumberSequence> ResetAsync(int id, long? nextValue = null, CancellationToken cancellationToken = default);
    }
}
