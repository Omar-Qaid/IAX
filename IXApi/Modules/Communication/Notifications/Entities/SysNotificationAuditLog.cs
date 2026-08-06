using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Communication.Notifications.Entities
{
    /// <summary>
    /// Tracks a delivery attempt of a notification through a specific channel to a user.
    /// Stores the delivery status, timestamp, external gateway response, or failure error message.
    /// </summary>
    [Table("SysNotificationAuditLogs")]
    public class SysNotificationAuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecId { get; set; }

        public long NotificationId { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;

        public SysNotificationChannel Channel { get; set; }

        public SysDeliveryStatus DeliveryStatus { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? ResponsePayload { get; set; }

        public string? ErrorMessage { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public virtual SysNotification Notification { get; set; } = null!;
    }
}

