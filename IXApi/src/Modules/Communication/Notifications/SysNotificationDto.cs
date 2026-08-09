using System.Text.Json.Serialization;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications
{
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
}