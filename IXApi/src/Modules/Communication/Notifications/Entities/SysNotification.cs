using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications.Entities
{
    /// <summary>
    /// Central notification entity for the enterprise notification system.
    /// Supports multi-tenant, multi-channel, template-based notifications with
    /// priority, categorization, and entity linking for any module.
    /// </summary>
    public class SysNotification : Entity<long>
    {
        /// <summary>
        /// Tenant identifier for multi-tenant isolation.
        /// </summary>
        [MaxLength(256)]
        public string? TenantId { get; set; }

        /// <summary>
        /// Links to the source entity's primary key (e.g. a WfRequest RecId, an Invoice Id).
        /// </summary>
        [MaxLength(256)]
        public string? EntityId { get; set; }

        /// <summary>
        /// Type name of the source entity (e.g. "WfRequest", "ARInvoice", "OrgEmployee").
        /// </summary>
        [MaxLength(256)]
        public string? EntityType { get; set; }

        /// <summary>
        /// Business reference number (e.g. Request #, Invoice #, PO #).
        /// </summary>
        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Notification title / subject line.
        /// </summary>
        [MaxLength(500)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Short notification message (shown in popups, badges, etc.).
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Extended description with additional context.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Icon identifier (MUI icon name or custom icon key).
        /// </summary>
        [MaxLength(100)]
        public string? Icon { get; set; }

        /// <summary>
        /// URL to an image associated with this notification.
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Deep-link URL the user navigates to when clicking the notification.
        /// </summary>
        [MaxLength(500)]
        public string? Url { get; set; }

        /// <summary>
        /// Notification priority level (Low, Medium, High, Critical).
        /// </summary>
        public SysNotificationPriority Priority { get; set; } = SysNotificationPriority.Medium;

        /// <summary>
        /// Category for grouping and filtering (e.g. "Workflow", "Finance", "HR", "System").
        /// </summary>
        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Delivery channel used for this notification.
        /// </summary>
        public SysNotificationChannel Channel { get; set; } = SysNotificationChannel.InApp;

        /// <summary>
        /// Overall notification lifecycle status.
        /// </summary>
        public SysNotificationStatus Status { get; set; } = SysNotificationStatus.Pending;

        /// <summary>
        /// When the notification expires and should no longer be displayed.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Optional reference to the template used to generate this notification.
        /// </summary>
        public int? TemplateId { get; set; }

        [ForeignKey(nameof(TemplateId))]
        [DeleteBehavior(DeleteBehavior.SetNull)]
        public virtual SysNotificationTemplate? Template { get; set; }

        /// <summary>
        /// Soft-delete flag.
        /// </summary>
        /// <summary>
        /// Collection of recipients for this notification.
        /// </summary>
        public virtual ICollection<SysNotificationRecipient> Recipients { get; set; } = new List<SysNotificationRecipient>();
    }
}


