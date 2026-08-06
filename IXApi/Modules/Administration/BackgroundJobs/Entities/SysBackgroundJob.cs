using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Entities
{
    /// <summary>
    /// A registered background job definition.
    /// Persisted in the SysBackgroundJobs table so jobs survive application restarts.
    /// Each job points to a handler (by <see cref="JobKey"/>) that contains the actual work.
    /// </summary>
    public class SysBackgroundJob : Entity<long>
    {
        /// <summary>
        /// Unique, human-readable name of the job (e.g. "Nightly Invoice Reminder").
        /// </summary>
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The handler key that resolves to an <c>ISysBackgroundJobHandler</c> implementation
        /// (e.g. "SendInvoiceReminders"). This is what actually runs.
        /// </summary>
        [MaxLength(200)]
        public string JobKey { get; set; } = null!;

        /// <summary>
        /// Optional description of what the job does.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Tenant identifier for multi-tenant isolation.
        /// </summary>
        [MaxLength(256)]
        public string? TenantId { get; set; }

        // ── Scheduling ───────────────────────────────────────────────────

        /// <summary>How the job is scheduled.</summary>
        public SysJobScheduleType ScheduleType { get; set; } = SysJobScheduleType.Recurring;

        /// <summary>
        /// Standard 5-field CRON expression (min hour day month day-of-week).
        /// Used when <see cref="ScheduleType"/> is <see cref="SysJobScheduleType.Cron"/>.
        /// </summary>
        [MaxLength(120)]
        public string? CronExpression { get; set; }

        /// <summary>
        /// Interval in seconds between runs for <see cref="SysJobScheduleType.Recurring"/> jobs.
        /// </summary>
        public int? IntervalSeconds { get; set; }

        /// <summary>
        /// Absolute time to run for <see cref="SysJobScheduleType.OneTime"/> /
        /// <see cref="SysJobScheduleType.Delayed"/> jobs.
        /// </summary>
        public DateTime? RunAt { get; set; }

        /// <summary>
        /// Computed UTC time of the next scheduled run. Null = nothing scheduled.
        /// </summary>
        public DateTime? NextRunAt { get; set; }

        // ── State ────────────────────────────────────────────────────────

        /// <summary>Lifecycle status (Active / Paused / Cancelled / Completed).</summary>
        public SysJobStatus Status { get; set; } = SysJobStatus.Active;

        /// <summary>Master enable switch. A disabled job is never scheduled.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// When true, the scheduler will not start a new execution while a previous
        /// one for the same job is still running (prevents overlapping runs).
        /// </summary>
        public bool PreventOverlap { get; set; } = true;

        // ── Reliability ──────────────────────────────────────────────────

        /// <summary>Maximum automatic retry attempts after a failure (0 = no retry).</summary>
        public int MaxRetryCount { get; set; } = 0;

        /// <summary>Base delay (seconds) before a retry. Backoff = delay * 2^(attempt-1).</summary>
        public int RetryDelaySeconds { get; set; } = 60;

        /// <summary>Maximum time (seconds) a single execution may run before being cancelled.</summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>JSON-serialized payload passed to the handler on each run.</summary>
        public string? PayloadJson { get; set; }

        // ── Tracking ─────────────────────────────────────────────────────

        /// <summary>Total number of times this job has been executed.</summary>
        public int RunCount { get; set; }

        /// <summary>UTC time of the most recent run start.</summary>
        public DateTime? LastRunAt { get; set; }

        /// <summary>Outcome of the most recent execution.</summary>
        public SysJobExecutionStatus? LastStatus { get; set; }

        /// <summary>Error message from the most recent failed execution.</summary>
        public string? LastError { get; set; }

        /// <summary>Soft-delete flag.</summary>
        public bool IsDeleted { get; set; }

        /// <summary>Execution history for this job.</summary>
        public virtual ICollection<SysBackgroundJobExecution> Executions { get; set; }
            = new List<SysBackgroundJobExecution>();
    }
}

