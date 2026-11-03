using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;

internal static class SysBackgroundJobActivePeriodDemoData
{
    internal static SysBackgroundJobActivePeriod Create(string code, string name, int from, int to) =>
        new SysBackgroundJobActivePeriod { Code = code, Name = name, FromTimeUtc = from, ToTimeUtc = to, FromTimeLocal = from, ToTimeLocal = to, TimeZoneFollowed = 0 };
}
