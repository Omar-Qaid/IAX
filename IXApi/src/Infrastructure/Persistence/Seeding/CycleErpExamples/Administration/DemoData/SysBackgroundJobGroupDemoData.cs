using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;

internal static class SysBackgroundJobGroupDemoData
{
    internal static SysBackgroundJobGroup Create(string code, string description, int concurrency) =>
        new SysBackgroundJobGroup { GroupCode = code, Description = description, MaxConcurrency = concurrency, SchedulingPriority = 1 };
}
