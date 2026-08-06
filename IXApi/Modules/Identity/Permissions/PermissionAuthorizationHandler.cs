using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    // ─── Requirement ─────────────────────────────────────────────────────────────
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission) => Permission = permission;
    }

    // ─── Handler ─────────────────────────────────────────────────────────────────
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

    // ─── Dynamic policy provider ─────────────────────────────────────────────────
    // Resolves any policy named  "permission:Module.Action"  on the fly, so we don't
    // have to pre-register every combination in AddAuthorization().
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private const string PolicyPrefix = "permission:";
        private readonly DefaultAuthorizationPolicyProvider _fallback;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
            => _fallback = new DefaultAuthorizationPolicyProvider(options);

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName[PolicyPrefix.Length..];
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
            return _fallback.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallback.GetFallbackPolicyAsync();
    }
}
