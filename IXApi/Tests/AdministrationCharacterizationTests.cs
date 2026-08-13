using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Administration.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class AdministrationCharacterizationTests
{
    [Theory]
    [InlineData(SysJobScheduleType.Cron, null, null, null, null, "CronExpression is required for Cron jobs.")]
    [InlineData(SysJobScheduleType.Cron, "invalid", null, null, null, "Invalid CRON expression: 'invalid'.")]
    [InlineData(SysJobScheduleType.Recurring, null, 0, null, null, "IntervalSeconds must be greater than 0 for Recurring jobs.")]
    [InlineData(SysJobScheduleType.OneTime, null, null, null, null, "RunAt is required for OneTime jobs.")]
    [InlineData(SysJobScheduleType.Delayed, null, null, null, null, "DelaySeconds (or RunAt) is required for Delayed jobs.")]
    public void Job_schedule_validation_preserves_existing_errors(
        SysJobScheduleType type,
        string? cron,
        int? intervalSeconds,
        DateTime? runAt,
        int? delaySeconds,
        string expected)
    {
        var actual = SysJobScheduleCalculator.ValidateSchedule(
            type, cron, intervalSeconds, runAt, delaySeconds);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Recurring_job_next_run_uses_interval_seconds()
    {
        var fromUtc = new DateTime(2026, 8, 13, 10, 0, 0, DateTimeKind.Utc);
        var job = new SysBackgroundJob
        {
            ScheduleType = SysJobScheduleType.Recurring,
            IntervalSeconds = 90
        };

        var next = SysJobScheduleCalculator.ComputeNextRun(job, fromUtc);

        Assert.Equal(fromUtc.AddSeconds(90), next);
    }

    [Fact]
    public void Number_sequence_format_prefers_annotated_format_and_replaces_date_tokens()
    {
        var sequence = new SysNumberSequence
        {
            NumberSequence = "TestEntity",
            Txt = "Test",
            Format = "IGNORED-{SEQ}",
            AnnotatedFormat = "DOC-{YYYY}{MM}{DD}-{SEQ}"
        };

        var code = SysNumberSequenceService.FormatCode(sequence, 42);
        var today = DateTime.UtcNow;

        Assert.Equal($"DOC-{today:yyyyMMdd}-00042", code);
    }

    [Fact]
    public void Settings_controller_contract_requires_authorization_and_keeps_route()
    {
        var controllerType = typeof(SysSettingsController);

        Assert.NotNull(controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).SingleOrDefault());
        var route = Assert.Single(controllerType.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>());
        Assert.Equal("api/v1/[controller]", route.Template);
    }
}
