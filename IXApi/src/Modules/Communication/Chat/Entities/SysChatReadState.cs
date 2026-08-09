using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Communication.Chat.Entities
{
    /// <summary>
    /// Tracks how far a user has read in a chat room. Unread = messages in the room sent by
    /// someone else after <see cref="LastReadAt"/>. One row per (user, room).
    /// </summary>
    public class SysChatReadState : Entity<long>, IAuditExempt
    {
        [MaxLength(256)]
        public string UserId { get; set; } = null!;

        [MaxLength(200)]
        public string RoomId { get; set; } = null!;

        public DateTime LastReadAt { get; set; }
    }
}

