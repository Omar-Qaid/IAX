using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace IAX.IXApi.Modules.Identity.Permissions;

public sealed class PermissionAuthorizationHandler(
    IAppPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var permissions = await permissionService.GetPermissionKeysByUserAsync(userId);
        if (permissions.Contains(requirement.Permission, StringComparer.Ordinal))
            context.Succeed(requirement);
    }
}
