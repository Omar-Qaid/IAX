using System.Text.Json.Serialization;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications
{
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
}