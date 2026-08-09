using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Admin role bypasses every permission check
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // "permission" claims are embedded in the JWT as "Module.Action" strings
            var hasClaim = context.User.Claims
                .Any(c => c.Type == "permission" && c.Value == requirement.Permission);

            if (hasClaim)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}