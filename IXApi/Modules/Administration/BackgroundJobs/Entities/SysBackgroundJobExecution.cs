using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Entities
{
    /// <summary>
    /// A single execution record (history/log entry) for a background job.
    /// One row is created per attempt, capturing timing, outcome and error details.
    /// </summary>
    public class SysBackgroundJobExecution
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecId { get; set; }

        /// <summary>The job this execution belongs to.</summary>
        public long JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual SysBackgroundJob? Job { get; set; }

        /// <summary>Attempt number (1 = first attempt, 2+ = retries).</summary>
        public int Attempt { get; set; } = 1;

        /// <summary>What caused this execution to start.</summary>
        public SysJobTrigger Trigger { get; set; } = SysJobTrigger.Schedule;

        /// <summary>The user who manually triggered the run (null for scheduled runs).</summary>
        [MaxLength(256)]
        public string? TriggeredByUserId { get; set; }

        /// <summary>Current status of the execution.</summary>
        public SysJobExecutionStatus Status { get; set; } = SysJobExecutionStatus.Pending;

        /// <summary>
        /// Earliest UTC time this pending execution may start. Null = immediately.
        /// Used to gate manual triggers and retry backoff without touching the job schedule.
        /// </summary>
        public DateTime? ScheduledFor { get; set; }

        /// <summary>UTC start time.</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>UTC completion time (success, failure or cancellation).</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Total execution duration in milliseconds.</summary>
        public long? DurationMs { get; set; }

        /// <summary>Optional free-form result/output produced by the handler.</summary>
        public string? Output { get; set; }

        /// <summary>Error message if the execution failed.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Stack trace / detailed error response if the execution failed.</summary>
        public string? ErrorDetail { get; set; }

        /// <summary>Name of the server/instance that ran the job (for scaled-out deployments).</summary>
        [MaxLength(256)]
        public string? ServerName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

