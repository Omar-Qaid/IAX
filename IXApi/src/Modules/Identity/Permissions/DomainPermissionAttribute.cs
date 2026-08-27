using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    /// <summary>
    /// Apply once per controller class. Maps HTTP method to View/Create/Edit/Delete,
    /// then checks the database for the user's current permissions.
    /// Checking the DB (not JWT claims) ensures changes take effect immediately
    /// without requiring users to re-login or refresh their token.
    ///
    /// Admin role bypasses all checks.
    ///
    /// Usage: [DomainPermission("Inventory", "Items")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DomainPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _module;
        private readonly string _resource;
        private readonly string? _action;

        public DomainPermissionAttribute(string module, string resource, string? action = null)
        {
            _module = module;
            _resource = resource;
            _action = action;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedObjectResult(APIResponse<object>.Fail("Authentication required."));
                return;
            }

            // Admin role bypasses all permission checks
            if (user.IsInRole("Admin"))
                return;

            var method = context.HttpContext.Request.Method.ToUpperInvariant();
            var action = _action ?? method switch
            {
                "GET" => "View",
                "POST" => "Create",
                "PUT" or "PATCH" => "Edit",
                "DELETE" => "Delete",
                _ => "View"
            };

            var requiredKey = $"{_module}.{_resource}.{action}";

            // Resolve user ID from JWT claims
            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedObjectResult(APIResponse<object>.Fail("Authentication required."));
                return;
            }

            // Always check the database — never rely on JWT claims for permissions.
            // This means permission changes take effect immediately without re-login.
            var permissionService = context.HttpContext.RequestServices
                .GetRequiredService<IAppPermissionService>();

            var userPermissions = await permissionService.GetPermissionKeysByUserAsync(userId);

            if (!userPermissions.Contains(requiredKey))
            {
                context.Result = new ObjectResult(APIResponse<object>.Fail($"Permission denied: {requiredKey}"))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
