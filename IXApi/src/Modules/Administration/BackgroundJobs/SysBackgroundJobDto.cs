using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
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
}