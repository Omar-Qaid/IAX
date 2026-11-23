using System.Security.Claims;
using IAX.IXApi.Api.Middleware;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class CompanyIsolationTests
{
    [Fact]
    public void Authenticated_user_defaults_to_their_authorized_company()
    {
        var context = CreateHttpContext("ksa");
        var executionContext = CreateExecutionContext(context);

        Assert.Equal("ksa", executionContext.GetDataAreaId());
        Assert.True(executionContext.IsRequestedCompanyAuthorized());
    }

    [Fact]
    public void Authorized_company_header_selects_matching_claim()
    {
        var context = CreateHttpContext("dat", "ksa");
        context.Request.Headers["X-Company"] = "ksa";
        var executionContext = CreateExecutionContext(context);

        Assert.Equal("ksa", executionContext.GetDataAreaId());
        Assert.True(executionContext.IsRequestedCompanyAuthorized());
    }

    [Theory]
    [InlineData("SystemAdmin")]
    [InlineData("Admin")]
    public void Administrator_can_select_any_well_formed_company(string role)
    {
        var context = CreateHttpContext("dat");
        context.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)]));
        context.Request.Headers["X-Company"] = "HBMC";
        var executionContext = CreateExecutionContext(context);

        Assert.Equal("HBMC", executionContext.GetDataAreaId());
        Assert.True(executionContext.IsRequestedCompanyAuthorized());
    }

    [Fact]
    public void Unauthorized_company_header_cannot_change_execution_context()
    {
        var context = CreateHttpContext("dat");
        context.Request.Headers["X-Company"] = "other";
        var executionContext = CreateExecutionContext(context);

        Assert.Equal("dat", executionContext.GetDataAreaId());
        Assert.False(executionContext.IsRequestedCompanyAuthorized());
    }

    [Theory]
    [InlineData("../dat")]
    [InlineData("dat;DROP TABLE Users")]
    [InlineData("company-id-that-is-too-long")]
    public void Malformed_company_header_is_rejected(string requestedCompany)
    {
        var context = CreateHttpContext("dat");
        context.Request.Headers["X-DataAreaId"] = requestedCompany;

        Assert.False(CreateExecutionContext(context).IsRequestedCompanyAuthorized());
    }

    [Fact]
    public void Multiple_company_header_values_are_rejected()
    {
        var context = CreateHttpContext("dat", "ksa");
        context.Request.Headers.Append("X-Company", "dat");
        context.Request.Headers.Append("X-Company", "ksa");

        Assert.False(CreateExecutionContext(context).IsRequestedCompanyAuthorized());
    }

    [Fact]
    public async Task Middleware_returns_forbidden_before_the_next_component()
    {
        var context = CreateHttpContext("dat");
        context.Request.Headers["X-Company"] = "other";
        var executionContext = CreateExecutionContext(context);
        var nextCalled = false;
        var middleware = new CompanySelectionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, executionContext);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public void Legacy_authenticated_users_without_company_claims_are_limited_to_dat()
    {
        var context = CreateHttpContext();
        var executionContext = CreateExecutionContext(context);

        Assert.Equal(CompanyContextDefaults.DataAreaId, executionContext.GetDataAreaId());

        context.Request.Headers["X-Company"] = "other";
        Assert.False(executionContext.IsRequestedCompanyAuthorized());
    }

    [Fact]
    public void Number_sequence_http_contract_does_not_accept_a_company_override()
    {
        Assert.Null(typeof(NextSequenceRequestDto).GetProperty("TenantId"));

        var peek = typeof(SysNumberSequenceController).GetMethod(nameof(SysNumberSequenceController.Peek));
        Assert.NotNull(peek);
        Assert.DoesNotContain(peek.GetParameters(), parameter =>
            parameter.Name?.Equals("tenantId", StringComparison.OrdinalIgnoreCase) == true);
    }

    private static DefaultHttpContext CreateHttpContext(params string[] companies)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "user-1") };
        claims.AddRange(companies.Select(company =>
            new Claim(CompanyContextDefaults.ClaimType, company)));

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
    }

    private static CompanyExecutionContext CreateExecutionContext(HttpContext context)
    {
        return new CompanyExecutionContext(new HttpContextAccessor
        {
            HttpContext = context
        });
    }
}
