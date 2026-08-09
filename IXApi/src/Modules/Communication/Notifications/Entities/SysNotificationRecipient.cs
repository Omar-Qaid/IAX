using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Identity.Users;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications.Entities
{
    /// <summary>
    /// Junction table linking a notification to its recipients.
    /// Each row represents one user who should receive a specific notification.
    /// </summary>
    public class SysNotificationRecipient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecId { get; set; }

        /// <summary>
        /// FK to the parent notification.
        /// </summary>
        public long NotificationId { get; set; }

        [ForeignKey(nameof(NotificationId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual SysNotification Notification { get; set; } = null!;

        /// <summary>
        /// FK to the recipient user.
        /// </summary>
        [MaxLength(256)]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AspNetUser User { get; set; } = null!;

        /// <summary>
        /// Whether this recipient has read the notification.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// When the recipient read the notification.
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// When the notification was delivered to this recipient's channel.
        /// </summary>
        public DateTime? DeliveredDate { get; set; }

        /// <summary>
        /// Per-recipient delivery status tracking.
        /// </summary>
        public SysDeliveryStatus DeliveryStatus { get; set; } = SysDeliveryStatus.Pending;

        /// <summary>
        /// Whether the notification has been archived by this recipient.
        /// </summary>
        public bool IsArchived { get; set; }
    }
}

