using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission) => Permission = permission;
    }
}