using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IAX.IXApi.Bootstrap.Extensions;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void Fallback_policy_requires_authenticated_users()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCustomAuthorization();
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Assert.NotNull(options.FallbackPolicy);
        Assert.Contains(options.FallbackPolicy!.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public async Task Permission_handler_uses_current_database_permissions()
    {
        var service = new StubPermissionService(["Finance.Currency.View"]);
        var handler = new PermissionAuthorizationHandler(service);
        var requirement = new PermissionRequirement("Finance.Currency.View");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, "user-1")], "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.Equal("user-1", service.LastRequestedUserId);
    }

    [Fact]
    public async Task Permission_handler_denies_permission_not_returned_by_database()
    {
        var handler = new PermissionAuthorizationHandler(new StubPermissionService([]));
        var requirement = new PermissionRequirement("Finance.Currency.Edit");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, "user-1")], "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private sealed class StubPermissionService(IReadOnlyCollection<string> permissions)
        : IAppPermissionService
    {
        public string? LastRequestedUserId { get; private set; }

        public Task<List<string>> GetPermissionKeysByUserAsync(
            string userId,
            CancellationToken ct = default)
        {
            LastRequestedUserId = userId;
            return Task.FromResult(permissions.ToList());
        }

        public Task<List<AppPermission>> GetAllAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<AppPermission>> GetByRoleAsync(
            string roleId,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AssignToRoleAsync(
            string roleId,
            IEnumerable<int> permissionIds,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task RemoveFromRoleAsync(
            string roleId,
            IEnumerable<int> permissionIds,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task SetRolePermissionsAsync(
            string roleId,
            IEnumerable<int> permissionIds,
            CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
