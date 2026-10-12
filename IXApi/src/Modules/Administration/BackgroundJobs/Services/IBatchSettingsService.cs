using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public interface IBatchSettingsService
{
    Task<BatchSettings> GetAsync(CancellationToken cancellationToken);
    Task<(BatchSettings? Settings, string? Error)> SaveAsync(BatchSettings settings, CancellationToken cancellationToken);
}
