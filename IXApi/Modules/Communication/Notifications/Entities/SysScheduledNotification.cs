using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Communication.Notifications.Entities
{
    /// <summary>
    /// Database-persisted scheduled notification job.
    /// Survives app restarts. Processed by SysNotificationBackgroundService.
    /// </summary>
    public class SysScheduledNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecId { get; set; }

        /// <summary>
        /// Type of scheduled job.
        /// </summary>
        public SysScheduledJobType JobType { get; set; } = SysScheduledJobType.Delayed;

        /// <summary>
        /// When the notification should be sent.
        /// </summary>
        public DateTime SendAt { get; set; }

        /// <summary>
        /// Current processing status.
        /// </summary>
        public SysScheduledJobStatus Status { get; set; } = SysScheduledJobStatus.Pending;

        // ── Notification Content ─────────────────────────────────────────

        [MaxLength(500)]
        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        [MaxLength(500)]
        public string? Url { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public SysNotificationPriority Priority { get; set; } = SysNotificationPriority.Medium;

        public SysNotificationChannel Channel { get; set; } = SysNotificationChannel.InApp;

        [MaxLength(256)]
        public string? EntityType { get; set; }

        [MaxLength(256)]
        public string? EntityId { get; set; }

        // ── Recipients ───────────────────────────────────────────────────

        /// <summary>
        /// Comma-separated list of user IDs to notify.
        /// </summary>
        public string? RecipientUserIds { get; set; }

        // ── Template ─────────────────────────────────────────────────────

        [MaxLength(100)]
        public string? TemplateCode { get; set; }

        /// <summary>
        /// JSON-serialized Dictionary of template placeholders.
        /// </summary>
        public string? TemplatePlaceholdersJson { get; set; }

        // ── Escalation ───────────────────────────────────────────────────

        /// <summary>
        /// For escalation jobs: the original notification to check read status.
        /// </summary>
        public long? OriginalNotificationId { get; set; }

        /// <summary>
        /// For escalation jobs: the user to check read status for.
        /// </summary>
        [MaxLength(256)]
        public string? EscalationUserId { get; set; }

        // ── Recurring ────────────────────────────────────────────────────

        /// <summary>
        /// Interval in minutes between recurring notifications.
        /// </summary>
        public int RecurringIntervalMinutes { get; set; }

        /// <summary>
        /// Maximum number of times a recurring job should fire. 0 = unlimited.
        /// </summary>
        public int MaxOccurrences { get; set; }

        /// <summary>
        /// Current occurrence count for recurring jobs.
        /// </summary>
        public int CurrentOccurrence { get; set; }

        // ── Tracking ─────────────────────────────────────────────────────

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
    }

    public enum SysScheduledJobType
    {
        Delayed = 0,
        Reminder = 1,
        Escalation = 2,
        Recurring = 3,
        FollowUp = 4
    }

    public enum SysScheduledJobStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3
    }
}

