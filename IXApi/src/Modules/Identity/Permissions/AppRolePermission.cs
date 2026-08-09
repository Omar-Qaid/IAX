using IAX.IXApi.Modules.Identity.Roles;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    public class AppRolePermission
    {
        [StringLength(450)]
        public string RoleId { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public virtual AspNetRole Role { get; set; } = null!;
        public virtual AppPermission Permission { get; set; } = null!;
    }
}
