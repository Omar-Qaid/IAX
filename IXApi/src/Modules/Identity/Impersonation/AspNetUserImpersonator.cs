using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Identity.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Identity.Impersonation
{
    public class AspNetUserImpersonator
    {

        public string ImpersonatorId { get; set; } = null!;
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(ImpersonatorId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AspNetUser AspNetImpersonator { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual AspNetUser AspNetUser { get; set; } = default!;

        public bool IsActive { get; set; }
        public bool UseOwnerRoles { get; set; }

        public DateTime ExpireDate { get; set; } 
       
    }
}

