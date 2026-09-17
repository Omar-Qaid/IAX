using System.Net.Http.Headers;

namespace IAX.IXMcp.Security;

public sealed class McpAuthenticationMiddleware(RequestDelegate next)
{
    public const string ContextItemKey = "IXMcp.SessionContext";

    public async Task InvokeAsync(HttpContext httpContext, IMcpSessionValidator validator)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/mcp"))
        {
            await next(httpContext);
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
        return context.Response.WriteAsJsonAsync(new
        {
            type = "about:blank",
            title = "MCP authentication failed",
            status = statusCode,
            code
        }, context.RequestAborted);
    }
}
