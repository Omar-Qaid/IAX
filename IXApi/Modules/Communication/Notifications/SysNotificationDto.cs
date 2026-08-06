using System.Text.Json.Serialization;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications
{
    // ────────────────────────────────────────────────────────────────────────────
    // Notification DTOs
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Outbound DTO for notification responses.
    /// </summary>
    public class SysNotificationDto
    {
        public long RecId { get; set; }
        public string? TenantId { get; set; }
        public string? EntityId { get; set; }
        public string? EntityType { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? ImageUrl { get; set; }
        public string? Url { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SysNotificationPriority Priority { get; set; }
        public string? Category { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SysNotificationChannel Channel { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SysNotificationStatus Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Per-recipient state (populated for the requesting user)
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool IsArchived { get; set; }

        public List<SysNotificationRecipientDto>? Recipients { get; set; }
    }

    /// <summary>
    /// Outbound DTO for a notification recipient.
    /// </summary>
    public class SysNotificationRecipientDto
    {
        public long RecId { get; set; }
        public string UserId { get; set; } = null!;
        public string? UserName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string DeliveryStatus { get; set; } = null!;
        public bool IsArchived { get; set; }
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Create / Send DTO
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inbound DTO for creating / sending a notification.
    /// Supports direct user targeting, role-based, department-based, and group-based recipients.
    /// </summary>
    public class CreateSysNotificationDto
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? ImageUrl { get; set; }
        public string? Url { get; set; }

        /// <summary>
        /// Priority level: Low, Medium, High, Critical.
        /// </summary>
        public SysNotificationPriority Priority { get; set; } = SysNotificationPriority.Medium;

        /// <summary>
        /// Category for grouping (e.g. "Workflow", "Finance", "HR", "System").
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Delivery channel: InApp, Email, SMS, Push, WhatsApp, MicrosoftTeams, Slack.
        /// </summary>
        public SysNotificationChannel Channel { get; set; } = SysNotificationChannel.InApp;

        /// <summary>
        /// Link to source entity.
        /// </summary>
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// When the notification should expire.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        // ── Recipient resolution ──────────────────────────────────────────────

        /// <summary>
        /// Direct user IDs to send to.
        /// </summary>
        public List<string>? UserIds { get; set; }

        /// <summary>
        /// Role names — all users in these roles will receive the notification.
        /// </summary>
        public List<string>? RoleNames { get; set; }

        /// <summary>
        /// Department IDs — all employees in these departments will receive the notification.
        /// </summary>
        public List<long>? DepartmentIds { get; set; }

        /// <summary>
        /// Employee group IDs — all employees in these groups will receive the notification.
        /// </summary>
        public List<long>? GroupIds { get; set; }

        // ── Template-based ────────────────────────────────────────────────────

        /// <summary>
        /// Optional template code. When provided, Title and Message are generated from the template.
        /// </summary>
        public string? TemplateCode { get; set; }

        /// <summary>
        /// Placeholder values for template substitution.
        /// e.g. { "UserName": "John", "RequestNumber": "REQ-001" }
        /// </summary>
        public Dictionary<string, string>? TemplatePlaceholders { get; set; }
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Template DTO
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// DTO for notification template CRUD operations.
    /// </summary>
    public class SysNotificationTemplateDto
    {
        public int RecId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NameAR { get; set; }
        public string Subject { get; set; } = null!;
        public string? SubjectAR { get; set; }
        public string Body { get; set; } = null!;
        public string? BodyAR { get; set; }
        public string? Variables { get; set; }
        public string? Icon { get; set; }
        public SysNotificationPriority DefaultPriority { get; set; }
        public string? DefaultCategory { get; set; }
        public SysNotificationChannel DefaultChannel { get; set; }
        public string Language { get; set; } = "all";
        public bool IsActive { get; set; }
    }
}

