using System.Text;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using Xunit;

namespace IXApi.Tests;

public class BatchRecurrenceEncodingTests
{
    [Fact]
    public void IntervalSupportsTextAndExistingBinarySeeds()
    {
        var now = new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc);
        foreach (var data in new[] { Encoding.UTF8.GetBytes("900"), BitConverter.GetBytes(900) })
        {
            var job = new SysBackgroundJob { ScheduleType = SysJobScheduleType.Recurring, RecurrenceData = data };
            Assert.Equal(now.AddMinutes(15), SysJobScheduleCalculator.ComputeNextRun(job, now));
        }
    }

    [Fact]
    public void InvalidCronIsRejected()
    {
        Assert.NotNull(SysJobScheduleCalculator.ValidateSchedule(SysJobScheduleType.Cron, Encoding.UTF8.GetBytes("invalid"), null, null));
    }
}
