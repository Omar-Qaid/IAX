using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Entities
{
    /// <summary>
    /// A registered background job definition.
    /// Persisted in the SysBackgroundJobs table so jobs survive application restarts.
    /// Each job points to a handler (by <see cref="JobKey"/>) that contains the actual work.
    /// </summary>
    public class SysBackgroundJob : Entity<long>
    {
        // ── Replaced / Requested Fields ──────────────────────────────────
        
        [MaxLength(200)]
        public string Caption { get; set; } = null!; // Replaces 'Name'

        [MaxLength(20)]
        public string? CanceledBy { get; set; }

        [MaxLength(8)]
        public string? DataPartition { get; set; }

        public DateTime? EndDateTime { get; set; } // Replaces 'LastRunAt'
        public int? EndDateTimeTzId { get; set; }
        
        public int Finishing { get; set; }
        public int LogLevel { get; set; }

        public DateTime? OrigStartDateTime { get; set; }
        public int? OrigStartDateTimeTzId { get; set; }

        public byte[]? RecurrenceData { get; set; } // Replaces 'CronExpression' and 'IntervalSeconds'
        
        public int RuntimeJob { get; set; }

        public DateTime? StartDateTime { get; set; } // Replaces 'RunAt' and 'NextRunAt'
        public int? StartDateTimeTzId { get; set; }

        public DateTime? StartDate { get; set; }
        public int? StartTime { get; set; }

        public int Critical { get; set; }
        public int MonitoringCategory { get; set; }
        public int Managed { get; set; }

        [MaxLength(20)]
        public string? ExecutingBy { get; set; }

        [MaxLength(10)]
        public string? ActivePeriod { get; set; }

        public int SchedulingPriority { get; set; } = 1; // Replaces 'Priority'
        public int SchedulingPriorityIsOverridden { get; set; }

        [MaxLength(10)]
        public string? BatchGroup { get; set; }
        public int EmitBusinessEvent { get; set; }

        // ── Retained Essential Framework Fields ──────────────────────────

        [MaxLength(200)]
        public string JobKey { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(256)]
        public string? TenantId { get; set; }

        public SysJobScheduleType ScheduleType { get; set; } = SysJobScheduleType.Recurring;
        public SysJobStatus Status { get; set; } = SysJobStatus.Active;
        public bool IsEnabled { get; set; } = true;
        public bool PreventOverlap { get; set; } = true;
        
        public int MaxRetryCount { get; set; } = 0;
        public int RetryDelaySeconds { get; set; } = 60;
        public int TimeoutSeconds { get; set; } = 300;
        public string? PayloadJson { get; set; }
        
        public int RunCount { get; set; }
        public SysJobExecutionStatus? LastStatus { get; set; }
        public string? LastError { get; set; }

        public virtual ICollection<SysBackgroundJobExecution> Executions { get; set; }
            = new List<SysBackgroundJobExecution>();

        [ForeignKey(nameof(BatchGroup))]
        public virtual SysBackgroundJobGroup? GroupNavigation { get; set; }

        [ForeignKey(nameof(ActivePeriod))]
        public virtual SysBackgroundJobActivePeriod? ActivePeriodNavigation { get; set; }

        public virtual ICollection<SysBackgroundJobRecurrenceCount> RecurrenceCounts { get; set; }
            = new List<SysBackgroundJobRecurrenceCount>();

    }

    public class SysBackgroundJobExecution : BaseEntity<long>
    {
        public long JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual SysBackgroundJob? Job { get; set; }

        public int Attempt { get; set; } = 1;
        public SysJobTrigger Trigger { get; set; } = SysJobTrigger.Schedule;

        [MaxLength(256)]
        public string? TriggeredByUserId { get; set; }

        public SysJobExecutionStatus Status { get; set; } = SysJobExecutionStatus.Pending;
        public DateTime? ScheduledFor { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? DurationMs { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        [MaxLength(256)]
        public string? ServerName { get; set; }

        public int AlertsProcessed { get; set; }

        [MaxLength(20)]
        public string? BatchCreatedBy { get; set; }

        [MaxLength(20)]
        public string? CanceledBy { get; set; }

        [MaxLength(200)]
        public string? Caption { get; set; }

        [MaxLength(8)]
        public string? DataPartition { get; set; }

        public int? EndDateTimeTzId { get; set; }
        public int Finishing { get; set; }
        public DateTime? OrigStartDateTime { get; set; }
        public int? OrigStartDateTimeTzId { get; set; }
        public int? StartDateTimeTzId { get; set; }

        [MaxLength(20)]
        public string? ExecutedBy { get; set; }

        public int RuntimeJob { get; set; }

        [MaxLength(10)]
        public string? BatchGroup { get; set; }

        public int GroupSchedulingPriority { get; set; }
        public int JobSchedulingPriority { get; set; }
        public int JobSchedulingPriorityIsOverridden { get; set; }
    }

    public class SysBackgroundJobActivePeriod : BaseEntity<long>
    {
        [MaxLength(10)]
        public string Code { get; set; } = null!;

        [MaxLength(150)]
        public string? Name { get; set; }

        public int FromTimeUtc { get; set; }
        public int ToTimeUtc { get; set; }
        public int FromTimeLocal { get; set; }
        public int ToTimeLocal { get; set; }
        public int TimeZoneFollowed { get; set; }
    }

    public class SysBackgroundJobGroup : BaseEntity<long>
    {
        [MaxLength(10)]
        public string GroupCode { get; set; } = null!;

        [MaxLength(60)]
        public string? Description { get; set; }

        public int SchedulingPriority { get; set; }
        public int MaxConcurrency { get; set; }
    }

    public class SysBackgroundJobRecurrenceCount : BaseEntity<long>
    {
        public long BatchJobId { get; set; }

        [ForeignKey(nameof(BatchJobId))]
        public virtual SysBackgroundJob? BatchJob { get; set; }

        public int RecurrenceCount { get; set; }
    }
}


