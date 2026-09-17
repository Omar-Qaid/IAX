using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IAX.IXMcp.Execution;
using IAX.IXMcp.Security;
using IAX.IXMcp.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Tests;

public sealed class IXApiSessionValidatorTests
{
    [Fact]
    public async Task Validates_user_and_company_through_IXApi_me()
    {
        HttpRequestMessage? captured = null;
        var validator = CreateValidator(request =>
        {
            captured = request;
            return Json(HttpStatusCode.OK, """
                {"success":true,"data":{"id":"user-a","userName":"alice","roles":["User"],"permissions":["Workflow.Requests.View"],"allowedCompanies":["DAT","USMF"]}}
                """);
        });

        var result = await validator.ValidateAsync("token-a", "USMF");

        Assert.True(result.IsValid);
        Assert.Equal("user-a", result.Context?.UserId);
        Assert.Equal("USMF", result.Context?.Company);
        Assert.Equal("Bearer", captured?.Headers.Authorization?.Scheme);
        Assert.Equal("token-a", captured?.Headers.Authorization?.Parameter);
        Assert.Equal("USMF", captured?.Headers.GetValues("X-Company").Single());
        Assert.Equal("/api/v1/Auth/me", captured?.RequestUri?.AbsolutePath);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, 401, "invalid_or_expired_token")]
    [InlineData(HttpStatusCode.Forbidden, 403, "company_access_denied")]
    [InlineData(HttpStatusCode.InternalServerError, 503, "identity_dependency_unavailable")]
    public async Task Maps_identity_failures(HttpStatusCode downstream, int expectedStatus, string expectedCode)
    {
        var validator = CreateValidator(_ => Json(downstream, "{}"));

        var result = await validator.ValidateAsync("token", "DAT");

        Assert.False(result.IsValid);
        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.Equal(expectedCode, result.ErrorCode);
    }

    [Fact]
    public async Task Rejects_company_not_returned_for_user()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.OK, """
            {"success":true,"data":{"id":"user-a","userName":"alice","roles":[],"permissions":[],"allowedCompanies":["DAT"]}}
            """));

        var result = await validator.ValidateAsync("token", "USMF");

        Assert.False(result.IsValid);
        Assert.Equal("company_access_denied", result.ErrorCode);
    }

    [Fact]
    public async Task Rejects_malformed_identity_response_without_throwing()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.OK, "{not-json"));

        var result = await validator.ValidateAsync("token", "DAT");

        Assert.False(result.IsValid);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal("invalid_identity_response", result.ErrorCode);
    }

    [Fact]
    public async Task Rejects_wrong_identity_field_types_without_throwing()
    {
        var validator = CreateValidator(_ => Json(HttpStatusCode.OK, """
            {"success":"true","data":{"id":"user-a","userName":"alice","allowedCompanies":["DAT"]}}
            """));

        var result = await validator.ValidateAsync("token", "DAT");

        Assert.False(result.IsValid);
        Assert.Equal("invalid_identity_response", result.ErrorCode);
    }

    [Fact]
    public async Task Parallel_sessions_keep_user_and_company_isolated()
    {
        var validator = CreateValidator(request =>
        {
            var token = request.Headers.Authorization!.Parameter!;
            var company = request.Headers.GetValues("X-Company").Single();
            return Json(HttpStatusCode.OK, JsonSerializer.Serialize(new
            {
                success = true,
                data = new
                {
                    id = token,
                    userName = token,
                    roles = Array.Empty<string>(),
                    permissions = Array.Empty<string>(),
                    allowedCompanies = new[] { company }
                }
            }));
        });

        var first = validator.ValidateAsync("user-a", "DAT");
        var second = validator.ValidateAsync("user-b", "USMF");
        var results = await Task.WhenAll(first, second);

        Assert.Equal("user-a", results[0].Context?.UserId);
        Assert.Equal("DAT", results[0].Context?.Company);
        Assert.Equal("user-b", results[1].Context?.UserId);
        Assert.Equal("USMF", results[1].Context?.Company);
    }

    private static IXApiSessionValidator CreateValidator(Func<HttpRequestMessage, HttpResponseMessage> response)
    {
        var client = new HttpClient(new StubHandler(response))
        {
            BaseAddress = new Uri("https://ixapi.test")
        };
        return new IXApiSessionValidator(
            new IXApiHttpClient(client),
            Options.Create(new IXMcpOptions
            {
                ContractDirectory = "unused",
                IXApiBaseUrl = "https://ixapi.test",
                CallTimeoutSeconds = 5,
                MaximumResponseBytes = 1024 * 1024
            }));
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string content) => new(status)
    {
        Content = new StringContent(content, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(response(request));
    }
}
