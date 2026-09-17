using System.Net.Http.Headers;
using IAX.IXMcp.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Security;

public sealed class McpAuthenticationMiddleware(
    RequestDelegate next,
    IOptions<IXMcpOptions> options)
{
    public const string ContextItemKey = "IXMcp.SessionContext";

    public async Task InvokeAsync(HttpContext httpContext, IMcpSessionValidator validator)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/mcp"))
        {
            await next(httpContext);
            return;
        }

        if (httpContext.Request.ContentLength > options.Value.MaximumRequestBodyBytes)
        {
            await WriteFailureAsync(httpContext, StatusCodes.Status413PayloadTooLarge, "request_too_large");
            return;
        }

        if (!IsAllowedOrigin(httpContext.Request.Headers.Origin.ToString()))
        {
            await WriteFailureAsync(httpContext, StatusCodes.Status403Forbidden, "origin_not_allowed");
            return;
        }

        var authorization = httpContext.Request.Headers.Authorization.ToString();
        var company = httpContext.Request.Headers["X-Company"].ToString();
        if (!AuthenticationHeaderValue.TryParse(authorization, out var header)
            || !header.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(header.Parameter)
            || string.IsNullOrWhiteSpace(company))
        {
            await WriteFailureAsync(httpContext, StatusCodes.Status401Unauthorized, "authentication_required");
            return;
        }

        SessionValidationResult result;
        try
        {
            result = await validator.ValidateAsync(header.Parameter, company, httpContext.RequestAborted);
        }
        catch (HttpRequestException)
        {
            await WriteFailureAsync(httpContext, StatusCodes.Status503ServiceUnavailable, "identity_dependency_unavailable");
            return;
        }
        catch (OperationCanceledException) when (!httpContext.RequestAborted.IsCancellationRequested)
        {
            await WriteFailureAsync(httpContext, StatusCodes.Status503ServiceUnavailable, "identity_dependency_timeout");
            return;
        }

        if (!result.IsValid || result.Context is null)
        {
            await WriteFailureAsync(httpContext, result.StatusCode, result.ErrorCode);
            return;
        }

        httpContext.Items[ContextItemKey] = result.Context;
        await next(httpContext);
    }

    private static Task WriteFailureAsync(HttpContext context, int statusCode, string code)
    {
        context.Response.StatusCode = statusCode;
        if (statusCode == StatusCodes.Status401Unauthorized)
        {
            context.Response.Headers.WWWAuthenticate = "Bearer";
        }
        return context.Response.WriteAsJsonAsync(new
        {
            type = "about:blank",
            title = "MCP authentication failed",
            status = statusCode,
            code
        }, context.RequestAborted);
    }

    private bool IsAllowedOrigin(string origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return true;
        }

        return options.Value.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
