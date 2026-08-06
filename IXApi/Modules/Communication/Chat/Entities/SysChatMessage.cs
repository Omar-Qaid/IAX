using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Communication.Chat.Entities
{
    /// <summary>
    /// A persisted chat message scoped to a room/conversation. Delivered in real time over
    /// <c>SysChatHub</c> and retrievable as history via the chat service/controller.
    /// </summary>
    public class SysChatMessage : Entity<long>, IAuditExempt
    {
        /// <summary>Logical room / conversation identifier (e.g. "req-123", or a DM pair key).</summary>
        [MaxLength(200)]
        public string RoomId { get; set; } = null!;

        /// <summary>Sender user RecId (Identity user).</summary>
        [MaxLength(256)]
        public string SenderId { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}


