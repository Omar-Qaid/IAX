using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using IAX.IXMcp.Execution;
using IAX.IXMcp.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Security;

public sealed class IXApiSessionValidator(
    IXApiHttpClient apiClient,
    IOptions<IXMcpOptions> options) : IMcpSessionValidator
{
    public async Task<SessionValidationResult> ValidateAsync(
        string accessToken,
        string company,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken)
            || string.IsNullOrWhiteSpace(company)
            || ContainsHeaderBreak(accessToken)
            || ContainsHeaderBreak(company))
        {
            return SessionValidationResult.Failure(StatusCodes.Status401Unauthorized, "invalid_session_context");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.TryAddWithoutValidation("X-Company", company);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.CallTimeoutSeconds));
        using HttpResponseMessage response = await apiClient.SendAsync(request, timeout.Token);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SessionValidationResult.Failure(StatusCodes.Status401Unauthorized, "invalid_or_expired_token");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return SessionValidationResult.Failure(StatusCodes.Status403Forbidden, "company_access_denied");
        }

        if (!response.IsSuccessStatusCode)
        {
            return SessionValidationResult.Failure(StatusCodes.Status503ServiceUnavailable, "identity_dependency_unavailable");
        }

        try
        {
            await response.Content.LoadIntoBufferAsync(options.Value.MaximumResponseBytes, timeout.Token);
        }
        catch (HttpRequestException)
        {
            return SessionValidationResult.Failure(StatusCodes.Status503ServiceUnavailable, "identity_response_too_large");
        }

        JsonDocument document;
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            document = await JsonDocument.ParseAsync(stream, new JsonDocumentOptions { MaxDepth = 32 }, timeout.Token);
        }
        catch (JsonException)
        {
            return SessionValidationResult.Failure(StatusCodes.Status503ServiceUnavailable, "invalid_identity_response");
        }

        using (document)
        {
            var root = document.RootElement;
            if (!root.TryGetProperty("success", out var success)
                || success.ValueKind != JsonValueKind.True
                || !root.TryGetProperty("data", out var data)
                || data.ValueKind != JsonValueKind.Object
                || !TryString(data, "id", out var userId)
                || !TryString(data, "userName", out var userName))
            {
                return SessionValidationResult.Failure(StatusCodes.Status503ServiceUnavailable, "invalid_identity_response");
            }

            var allowedCompanies = ReadStrings(data, "allowedCompanies");
            if (!allowedCompanies.Contains("*", StringComparer.OrdinalIgnoreCase)
                && !allowedCompanies.Contains(company, StringComparer.OrdinalIgnoreCase))
            {
                return SessionValidationResult.Failure(StatusCodes.Status403Forbidden, "company_access_denied");
            }

            return SessionValidationResult.Success(new McpSessionContext(
                userId,
                userName,
                accessToken,
                company,
                ReadStrings(data, "roles"),
                ReadStrings(data, "permissions")));
        }
    }

    private static bool ContainsHeaderBreak(string value) => value.Contains('\r') || value.Contains('\n');

    private static bool TryString(JsonElement parent, string property, out string value)
    {
        value = string.Empty;
        return parent.TryGetProperty(property, out var element)
            && element.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(value = element.GetString()!);
    }

    private static string[] ReadStrings(JsonElement parent, string property) =>
        parent.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.Array
            ? element.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray()
            : [];
}
