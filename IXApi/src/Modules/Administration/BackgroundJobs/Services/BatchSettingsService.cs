using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public sealed class BatchSettingsService(IUnitOfWork unitOfWork) : IBatchSettingsService
{
    public async Task<BatchSettings> GetAsync(CancellationToken cancellationToken) =>
        await unitOfWork.Context.Set<BatchSettings>().AsNoTracking()
            .SingleOrDefaultAsync(settings => settings.Id == 1, cancellationToken) ?? new();

    public async Task<(BatchSettings? Settings, string? Error)> SaveAsync(
        BatchSettings settings, CancellationToken cancellationToken)
    {
        if (settings.Id != 1) return (null, "Batch settings use singleton ID 1.");
        if (settings.PollIntervalSeconds is < 1 or > 3600)
            return (null, "Polling interval must be between 1 and 3600 seconds.");

        var set = unitOfWork.Context.Set<BatchSettings>();
        var stored = await set.SingleOrDefaultAsync(value => value.Id == 1, cancellationToken);
        if (stored == null) set.Add(settings);
        else
        {
            stored.Enabled = settings.Enabled;
            stored.PollIntervalSeconds = settings.PollIntervalSeconds;
        }
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return (stored ?? settings, null);
    }
}
