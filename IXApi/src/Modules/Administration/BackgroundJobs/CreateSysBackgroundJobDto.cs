using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
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
}