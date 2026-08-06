using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    /// <summary>Read model for a background job definition.</summary>
    public class SysBackgroundJobDto
    {
        public long RecId { get; set; }
        public string Name { get; set; } = null!;
        public string JobKey { get; set; } = null!;
        public string? Description { get; set; }
        public string? TenantId { get; set; }

        public SysJobScheduleType ScheduleType { get; set; }
        public string? CronExpression { get; set; }
        public int? IntervalSeconds { get; set; }
        public DateTime? RunAt { get; set; }
        public DateTime? NextRunAt { get; set; }

        public SysJobStatus Status { get; set; }
        public bool IsEnabled { get; set; }
        public bool PreventOverlap { get; set; }

        public int MaxRetryCount { get; set; }
        public int RetryDelaySeconds { get; set; }
        public int TimeoutSeconds { get; set; }
        public string? PayloadJson { get; set; }

        public int RunCount { get; set; }
        public DateTime? LastRunAt { get; set; }
        public SysJobExecutionStatus? LastStatus { get; set; }
        public string? LastError { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    /// <summary>Create payload for a new background job.</summary>
    public class CreateSysBackgroundJobDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(200)]
        public string JobKey { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public SysJobScheduleType ScheduleType { get; set; } = SysJobScheduleType.Recurring;

        /// <summary>Required when ScheduleType = Cron.</summary>
        public string? CronExpression { get; set; }

        /// <summary>Required when ScheduleType = Recurring.</summary>
        public int? IntervalSeconds { get; set; }

        /// <summary>For OneTime = absolute time; for Delayed = ignored if DelaySeconds set.</summary>
        public DateTime? RunAt { get; set; }

        /// <summary>For Delayed jobs: run after this many seconds from creation.</summary>
        public int? DelaySeconds { get; set; }

        public bool IsEnabled { get; set; } = true;
        public bool PreventOverlap { get; set; } = true;

        public int MaxRetryCount { get; set; } = 0;
        public int RetryDelaySeconds { get; set; } = 60;
        public int TimeoutSeconds { get; set; } = 300;

        public string? PayloadJson { get; set; }
    }

    /// <summary>Update payload for an existing job's schedule and reliability settings.</summary>
    public class UpdateSysBackgroundJobScheduleDto
    {
        public SysJobScheduleType ScheduleType { get; set; }
        public string? CronExpression { get; set; }
        public int? IntervalSeconds { get; set; }
        public DateTime? RunAt { get; set; }
        public int? DelaySeconds { get; set; }

        public bool? IsEnabled { get; set; }
        public bool? PreventOverlap { get; set; }
        public int? MaxRetryCount { get; set; }
        public int? RetryDelaySeconds { get; set; }
        public int? TimeoutSeconds { get; set; }
        public string? PayloadJson { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>Read model for a single execution record.</summary>
    public class SysBackgroundJobExecutionDto
    {
        public long RecId { get; set; }
        public long JobId { get; set; }
        public string? JobName { get; set; }
        public int Attempt { get; set; }
        public SysJobTrigger Trigger { get; set; }
        public string? TriggeredByUserId { get; set; }
        public SysJobExecutionStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? DurationMs { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ServerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Aggregated, dashboard-ready snapshot of the job subsystem.</summary>
    public class SysBackgroundJobDashboardDto
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int PausedJobs { get; set; }
        public int CancelledJobs { get; set; }

        public int RunningNow { get; set; }
        public int ExecutionsLast24h { get; set; }
        public int SucceededLast24h { get; set; }
        public int FailedLast24h { get; set; }
        public double SuccessRatePct { get; set; }
        public double AvgDurationMsLast24h { get; set; }

        public List<SysBackgroundJobDto> NextDueJobs { get; set; } = new();
        public List<SysBackgroundJobExecutionDto> RecentExecutions { get; set; } = new();
        public List<string> RegisteredHandlerKeys { get; set; } = new();
    }
}

