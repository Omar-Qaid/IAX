using System.Text.Json.Serialization;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications
{
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
}