using System.Reflection;
using IAX.IXApi.Modules.Identity.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class SecurityBaselineTests
{
    [Fact]
    public void Login_is_explicitly_anonymous()
    {
        var action = GetAction(nameof(AuthController.Login));

        Assert.NotNull(action.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void Register_requires_admin_role()
    {
        var action = GetAction(nameof(AuthController.Register));
        var authorize = action.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("Admin", authorize!.Roles);
        Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Theory]
    [InlineData(nameof(AuthController.ExternalLogin))]
    [InlineData(nameof(AuthController.ExternalLoginCallback))]
    [InlineData(nameof(AuthController.ExternalProviders))]
    public void External_auth_entry_points_are_explicitly_anonymous(string actionName)
    {
        Assert.NotNull(GetAction(actionName).GetCustomAttribute<AllowAnonymousAttribute>());
    }

    private static MethodInfo GetAction(string name) =>
        typeof(AuthController).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == name);
}
