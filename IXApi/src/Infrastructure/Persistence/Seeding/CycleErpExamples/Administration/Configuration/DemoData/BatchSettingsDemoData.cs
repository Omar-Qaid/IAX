using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;

internal static class BatchSettingsDemoData
{
    internal static BatchSettings Create(int pollInterval) =>
        new BatchSettings { Id = 1, Enabled = true, PollIntervalSeconds = Math.Clamp(pollInterval, 1, 3600) };
}
