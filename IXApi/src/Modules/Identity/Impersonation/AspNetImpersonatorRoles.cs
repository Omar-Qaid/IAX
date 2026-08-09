using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Identity.Impersonation
{
    public class AspNetImpersonatorRoles
    {
        public string? ImpersonatorId { get; set; }
        public string RoleId { get; set; } = null!;

        [ForeignKey(nameof(ImpersonatorId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public AspNetUser? AspNetImpersonator { get; set; }

        [ForeignKey(nameof(RoleId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public AspNetRole? AspNetRole { get; set; }
    }

}

