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

    [Fact]
    public void CalendarRecurrenceSupportsMonthsAndYears()
    {
        var now = new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc);
        var monthly = new SysBackgroundJob
        {
            ScheduleType = SysJobScheduleType.Recurring,
            RecurrenceData = Encoding.UTF8.GetBytes("{\"unit\":\"months\",\"interval\":1}")
        };
        var yearly = new SysBackgroundJob
        {
            ScheduleType = SysJobScheduleType.Recurring,
            RecurrenceData = Encoding.UTF8.GetBytes("{\"unit\":\"years\",\"interval\":2}")
        };

        Assert.Equal(new DateTime(2026, 2, 28, 12, 0, 0, DateTimeKind.Utc),
            SysJobScheduleCalculator.ComputeNextRun(monthly, now));
        Assert.Equal(now.AddYears(2), SysJobScheduleCalculator.ComputeNextRun(yearly, now));
    }

    [Fact]
    public void RecurrenceEndConditionsAreHonored()
    {
        var now = new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc);
        var endAfter = new SysBackgroundJob
        {
            ScheduleType = SysJobScheduleType.Recurring,
            RunCount = 3,
            RecurrenceData = Encoding.UTF8.GetBytes("{\"unit\":\"days\",\"interval\":1,\"endAfter\":3}")
        };
        var endBy = new SysBackgroundJob
        {
            ScheduleType = SysJobScheduleType.Recurring,
            RecurrenceData = Encoding.UTF8.GetBytes("{\"unit\":\"days\",\"interval\":2,\"endBy\":\"2026-09-16T00:00:00Z\"}")
        };

        Assert.Null(SysJobScheduleCalculator.ComputeNextRun(endAfter, now));
        Assert.Null(SysJobScheduleCalculator.ComputeNextRun(endBy, now));
    }
}
