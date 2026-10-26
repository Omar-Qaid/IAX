using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces
{
    public interface IBatchSettingsService : IBaseService<BatchSettings>
    {
        Task<BatchSettings> GetAsync(CancellationToken cancellationToken);
        Task<(BatchSettings? Settings, string? Error)> SaveAsync(BatchSettings settings, CancellationToken cancellationToken);
    }
}
