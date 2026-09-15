using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    public class CreateSysBackgroundJobDto
    {
        [Required, MaxLength(200)]
        public string Caption { get; set; } = null!;

        [Required, MaxLength(200)]
        public string JobKey { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public SysJobScheduleType ScheduleType { get; set; } = SysJobScheduleType.Recurring;

        public byte[]? RecurrenceData { get; set; }

        public DateTime? StartDateTime { get; set; }
        public int? StartDateTimeTzId { get; set; }
        public DateTime? StartDate { get; set; }
        public int? StartTime { get; set; }

        public int? DelaySeconds { get; set; }

        public bool IsEnabled { get; set; } = true;
        public bool PreventOverlap { get; set; } = true;

        public int SchedulingPriority { get; set; } = 1;
        public int Critical { get; set; }
        public int MonitoringCategory { get; set; }
        public int Managed { get; set; }
        public int EmitBusinessEvent { get; set; }

        [MaxLength(10)]
        public string? BatchGroup { get; set; }

        [MaxLength(10)]
        public string? ActivePeriod { get; set; }

        public int MaxRetryCount { get; set; } = 0;
        public int RetryDelaySeconds { get; set; } = 60;
        public int TimeoutSeconds { get; set; } = 300;

        public string? PayloadJson { get; set; }
    }
}
