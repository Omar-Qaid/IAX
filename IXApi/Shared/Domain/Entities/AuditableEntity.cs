using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

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
        [AdaptIgnore(MemberSide.Destination)]
        [ForeignKey(nameof(CreatedBy))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AspNetUser? CreatedByUser { get; set; }

        [AdaptIgnore(MemberSide.Destination)]
        [ForeignKey(nameof(LastModifiedBy))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AspNetUser? LastModifiedByUser { get; set; }

        [AdaptIgnore(MemberSide.Destination)]
        [ForeignKey(nameof(OwnerAccountId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AspNetUser? OwnerAccount { get; set; }
    }
}

