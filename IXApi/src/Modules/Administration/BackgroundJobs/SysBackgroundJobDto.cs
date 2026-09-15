using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    public class SysBackgroundJobDto
    {
        public long RecId { get; set; }
        
        public string Caption { get; set; } = null!;
        public string JobKey { get; set; } = null!;
        public string? Description { get; set; }
        public string? TenantId { get; set; }

        public SysJobScheduleType ScheduleType { get; set; }
        public byte[]? RecurrenceData { get; set; }

        public DateTime? StartDateTime { get; set; }
        public int? StartDateTimeTzId { get; set; }
        public DateTime? StartDate { get; set; }
        public int? StartTime { get; set; }
        public DateTime? OrigStartDateTime { get; set; }
        public int? OrigStartDateTimeTzId { get; set; }

        public DateTime? EndDateTime { get; set; }
        public int? EndDateTimeTzId { get; set; }
        
        public string? CanceledBy { get; set; }
        public string? DataPartition { get; set; }
        public int Finishing { get; set; }
        public int LogLevel { get; set; }
        public int RuntimeJob { get; set; }

        public SysJobStatus Status { get; set; }
        public bool IsEnabled { get; set; }
        public bool PreventOverlap { get; set; }

        public int SchedulingPriority { get; set; }
        public int SchedulingPriorityIsOverridden { get; set; }
        public int Critical { get; set; }
        public int MonitoringCategory { get; set; }
        public int Managed { get; set; }
        public string? ExecutingBy { get; set; }
        public string? ActivePeriod { get; set; }
        public string? BatchGroup { get; set; }
        public int EmitBusinessEvent { get; set; }

        public int MaxRetryCount { get; set; }
        public int RetryDelaySeconds { get; set; }
        public int TimeoutSeconds { get; set; }
        public string? PayloadJson { get; set; }

        public int RunCount { get; set; }
        public SysJobExecutionStatus? LastStatus { get; set; }
        public string? LastError { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
