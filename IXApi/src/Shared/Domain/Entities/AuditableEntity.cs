using System;

namespace IAX.IXApi.Shared.Domain.Entities
{
    public abstract class AuditableEntity : ICreatedBy
    {
        // These IDs are set by the server — never overwritten by client data (MemberSide.Destination = ignore inbound)
        public string? CreatedBy { get; set; } = null!;

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public string? LastModifiedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; } = DateTime.Now;

        public string? OwnerAccountId { get; set; } = null!;

        // Navigation properties — send outbound (Entity→DTO) but ignore inbound (DTO→Entity)
    }
}
