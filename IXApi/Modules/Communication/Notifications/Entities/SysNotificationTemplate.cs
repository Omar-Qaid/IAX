using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mapster;

namespace IAX.IXApi.Modules.Communication.Notifications.Entities
{
    /// <summary>
    /// Reusable notification templates with multi-language support.
    /// Templates use {{placeholder}} syntax for dynamic content substitution.
    /// Example codes: WF_APPROVED, INV_LOW_STOCK, HR_LEAVE_REQUEST, FIN_INVOICE_DUE
    /// </summary>
    public class SysNotificationTemplate : Entity<int>
    {
        /// <summary>
        /// Unique template identifier, e.g. "WF_APPROVED", "INV_LOW_STOCK".
        /// </summary>
        [MaxLength(100)]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Display name for the template.
        /// </summary>
        [MaxLength(256)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Arabic display name.
        /// </summary>
        [MaxLength(256)]
        public string? NameAR { get; set; }

        /// <summary>
        /// Notification subject/title template (English).
        /// Supports {{UserName}}, {{RequestNumber}}, etc.
        /// </summary>
        [MaxLength(500)]
        public string Subject { get; set; } = null!;

        /// <summary>
        /// Notification subject/title template (Arabic).
        /// </summary>
        [MaxLength(500)]
        public string? SubjectAR { get; set; }

        /// <summary>
        /// Notification body template (English).
        /// Supports {{UserName}}, {{RequestNumber}}, {{RequestStatus}}, etc.
        /// </summary>
        public string Body { get; set; } = null!;

        /// <summary>
        /// Notification body template (Arabic).
        /// </summary>
        public string? BodyAR { get; set; }

        /// <summary>
        /// Comma-separated list of available template variables.
        /// e.g. "UserName,RequestNumber,RequestStatus"
        /// </summary>
        [MaxLength(1000)]
        public string? Variables { get; set; }

        /// <summary>
        /// Default icon for notifications created from this template.
        /// </summary>
        [MaxLength(100)]
        public string? Icon { get; set; }

        /// <summary>
        /// Default priority for notifications created from this template.
        /// </summary>
        public SysNotificationPriority DefaultPriority { get; set; } = SysNotificationPriority.Medium;

        /// <summary>
        /// Default category for notifications created from this template.
        /// e.g. "Workflow", "Finance", "HR", "System"
        /// </summary>
        [MaxLength(100)]
        public string? DefaultCategory { get; set; }

        /// <summary>
        /// Default channel for notifications created from this template.
        /// </summary>
        public SysNotificationChannel DefaultChannel { get; set; } = SysNotificationChannel.InApp;

        /// <summary>
        /// Supported language code, e.g. "en", "ar", or "all".
        /// </summary>
        [MaxLength(10)]
        public string Language { get; set; } = "all";

        /// <summary>
        /// Whether this template is active and can be used.
        /// </summary>
    }
}


