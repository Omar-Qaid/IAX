using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using IAX.IXMcp.Catalog;
using IAX.IXMcp.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Execution;

public sealed class McpToolExecutor(
    McpCatalogState catalogState,
    IXApiHttpClient apiClient,
    ToolArgumentValidator argumentValidator,
    IOptions<IXMcpOptions> options,
    ILogger<McpToolExecutor> logger) : IMcpToolExecutor
{
    public async Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        JsonElement arguments,
        ToolExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var correlationId = NormalizeCorrelationId(context.CorrelationId);
        var tool = catalogState.Snapshot.Tools.SingleOrDefault(candidate => candidate.Name == toolName);
        if (tool is null)
        {
            return ToolExecutionResult.Failure("unknown_tool", "The requested tool is not available.", correlationId);
        }

        if (!tool.ExecutionEnabled)
        {
            return ToolExecutionResult.Failure("tool_disabled", "The requested tool is not enabled.", correlationId);
        }

        if (!IsSafeContext(context))
        {
            return ToolExecutionResult.Failure(
                "invalid_execution_context",
                "A valid delegated credential and company context are required.",
                correlationId);
        }

        if (Encoding.UTF8.GetByteCount(arguments.GetRawText()) > options.Value.MaximumInputBytes)
        {
            return ToolExecutionResult.Failure("input_too_large", "The tool input exceeds the configured limit.", correlationId);
        }

        var validationError = argumentValidator.Validate(arguments, tool.InputSchema);
        if (validationError is not null)
        {
            return ToolExecutionResult.Failure("invalid_arguments", validationError, correlationId);
        }

        string relativeUri;
        try
        {
            relativeUri = BindRelativeUri(tool, arguments);
        }
        catch (ArgumentException exception)
        {
            return ToolExecutionResult.Failure("invalid_arguments", exception.Message, correlationId);
        }

        if (!IsApprovedRelativeUri(relativeUri))
        {
            return ToolExecutionResult.Failure("route_not_allowed", "The compiled route is outside the approved API prefix.", correlationId);
        }

        using var request = new HttpRequestMessage(new HttpMethod(tool.Method), relativeUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        request.Headers.TryAddWithoutValidation("X-Company", context.Company);
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);
        if (Activity.Current is { Id: { } traceParent } activity)
        {
            request.Headers.TryAddWithoutValidation("traceparent", traceParent);
            if (!string.IsNullOrWhiteSpace(activity.TraceStateString))
            {
                request.Headers.TryAddWithoutValidation("tracestate", activity.TraceStateString);
            }
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.CallTimeoutSeconds));

        try
        {
            using var response = await apiClient.SendAsync(request, timeout.Token);
            var responseBytes = await ReadBoundedAsync(
                await response.Content.ReadAsStreamAsync(timeout.Token),
                options.Value.MaximumResponseBytes,
                timeout.Token);
            return NormalizeResponse(tool, context.Company, correlationId, response, responseBytes);
        }
        catch (ResponseLimitExceededException)
        {
            return ToolExecutionResult.Failure(
                "response_too_large",
                "The IXApi response exceeds the configured limit.",
                correlationId);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ToolExecutionResult.Failure("downstream_timeout", "The IXApi request timed out.", correlationId);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "IXApi transport failure for {OperationId} ({CorrelationId}).", tool.OperationId, correlationId);
            return ToolExecutionResult.Failure("downstream_unavailable", "IXApi is unavailable.", correlationId);
        }
    }

    private ToolExecutionResult NormalizeResponse(
        CompiledTool tool,
        string company,
        string correlationId,
        HttpResponseMessage response,
        byte[] bytes)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return Success(tool, company, correlationId, null, null, (int)response.StatusCode);
        }

        JsonNode? payload;
        try
        {
            payload = bytes.Length == 0
                ? null
                : JsonNode.Parse(bytes, documentOptions: new JsonDocumentOptions
                {
                    MaxDepth = options.Value.MaximumJsonDepth
                });
        }
        catch (JsonException)
        {
            return ToolExecutionResult.Failure(
                "invalid_downstream_response",
                "IXApi returned an invalid JSON response.",
                correlationId,
                (int)response.StatusCode);
        }

        if (!response.IsSuccessStatusCode)
        {
            return ToolExecutionResult.Failure(
                StatusCode(response.StatusCode),
                SafeErrorMessage(payload, response.StatusCode),
                correlationId,
                (int)response.StatusCode);
        }

        if (payload is not JsonObject envelope)
        {
            return ToolExecutionResult.Failure(
                "invalid_downstream_response",
                "IXApi returned an unexpected response envelope.",
                correlationId,
                (int)response.StatusCode);
        }

        if (envelope["success"]?.GetValue<bool>() != true)
        {
            return ToolExecutionResult.Failure(
                "business_failure",
                SafeErrorMessage(envelope, response.StatusCode),
                correlationId,
                (int)response.StatusCode);
        }

        JsonNode? projected;
        try
        {
            projected = ProjectData(envelope["data"], tool.AllowedDataFields);
        }
        catch (InvalidDataException)
        {
            return ToolExecutionResult.Failure(
                "invalid_downstream_response",
                "IXApi response data does not match the compiled tool contract.",
                correlationId,
                (int)response.StatusCode);
        }

        return Success(
            tool,
            company,
            correlationId,
            projected,
            ProjectPagination(envelope["pagination"]),
            (int)response.StatusCode);
    }

    private static ToolExecutionResult Success(
        CompiledTool tool,
        string company,
        string correlationId,
        JsonNode? data,
        JsonNode? pagination,
        int statusCode)
    {
        var result = new JsonObject
        {
            ["ok"] = true,
            ["data"] = data,
            ["meta"] = new JsonObject
            {
                ["company"] = company,
                ["correlationId"] = correlationId,
                ["operationId"] = tool.OperationId,
                ["contractVersion"] = tool.ContractVersion,
                ["downstreamStatus"] = statusCode
            }
        };
        if (pagination is not null)
        {
            result["pagination"] = pagination;
        }

        return ToolExecutionResult.Success(result);
    }

    private static JsonNode? ProjectData(JsonNode? data, IReadOnlyList<string> allowedFields)
    {
        if (data is null)
        {
            return null;
        }

        if (data is JsonArray array)
        {
            var projected = new JsonArray();
            foreach (var item in array)
            {
                projected.Add(ProjectObject(item, allowedFields));
            }

            return projected;
        }

        return ProjectObject(data, allowedFields);
    }

    private static JsonObject ProjectObject(JsonNode? value, IReadOnlyList<string> allowedFields)
    {
        if (value is not JsonObject source)
        {
            throw new InvalidDataException();
        }

        var result = new JsonObject();
        foreach (var field in allowedFields)
        {
            if (!source.TryGetPropertyValue(field, out var fieldValue))
            {
                throw new InvalidDataException();
            }

            result[field] = fieldValue?.DeepClone();
        }

        return result;
    }

    private static JsonNode? ProjectPagination(JsonNode? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is not JsonObject source)
        {
            throw new InvalidDataException();
        }

        var result = new JsonObject();
        foreach (var field in new[] { "pageNumber", "pageSize", "totalRecords", "totalPages" })
        {
            if (source.TryGetPropertyValue(field, out var fieldValue))
            {
                result[field] = fieldValue?.DeepClone();
            }
        }

        return result;
    }

    private string BindRelativeUri(CompiledTool tool, JsonElement arguments)
    {
        var path = tool.Path;
        var queryValues = new List<string>();

        foreach (var binding in tool.BindingParameters)
        {
            var section = arguments.TryGetProperty(binding.Location, out var sectionValue)
                ? sectionValue
                : default;
            if (section.ValueKind != JsonValueKind.Object
                || !section.TryGetProperty(binding.Name, out var value))
            {
                if (binding.Required)
                {
                    throw new ArgumentException($"arguments.{binding.Location}.{binding.Name} is required.");
                }

                continue;
            }

            var encoded = Uri.EscapeDataString(ToInvariantString(value));
            if (binding.Location == "path")
            {
                path = path.Replace($"{{{binding.Name}}}", encoded, StringComparison.Ordinal);
            }
            else if (binding.Location == "query")
            {
                queryValues.Add($"{Uri.EscapeDataString(binding.Name)}={encoded}");
            }
        }

        if (path.Contains('{') || path.Contains('}'))
        {
            throw new ArgumentException("A compiled path parameter was not bound.");
        }

        return queryValues.Count == 0 ? path : $"{path}?{string.Join('&', queryValues)}";
    }

    private bool IsApprovedRelativeUri(string value)
    {
        return value.StartsWith(options.Value.ApprovedPathPrefix, StringComparison.Ordinal)
            && !value.Contains("..", StringComparison.Ordinal)
            && Uri.TryCreate(value, UriKind.Relative, out _)
            && !value.StartsWith("//", StringComparison.Ordinal)
            && !value.Contains('\\');
    }

    private static bool IsSafeContext(ToolExecutionContext context) =>
        !string.IsNullOrWhiteSpace(context.AccessToken)
        && !string.IsNullOrWhiteSpace(context.Company)
        && !ContainsHeaderBreak(context.AccessToken)
        && !ContainsHeaderBreak(context.Company);

    private static bool ContainsHeaderBreak(string value) => value.Contains('\r') || value.Contains('\n');

    private static string NormalizeCorrelationId(string? value) =>
        string.IsNullOrWhiteSpace(value) || ContainsHeaderBreak(value)
            ? Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N")
            : value;

    private static string ToInvariantString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString()!,
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => throw new ArgumentException("Only scalar path and query values are supported.")
    };

    private static string StatusCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized => "downstream_unauthorized",
        HttpStatusCode.Forbidden => "downstream_forbidden",
        HttpStatusCode.NotFound => "downstream_not_found",
        HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity => "downstream_validation",
        _ => "downstream_failure"
    };

    private static string SafeErrorMessage(JsonNode? payload, HttpStatusCode statusCode)
    {
        if (payload is JsonObject error)
        {
            foreach (var property in new[] { "detail", "title", "message" })
            {
                if (error[property]?.GetValue<string>() is { Length: > 0 } message)
                {
                    return message.Length <= 512 ? message : message[..512];
                }
            }
        }

        return $"IXApi returned HTTP {(int)statusCode}.";
    }

    private static async Task<byte[]> ReadBoundedAsync(
        Stream stream,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream(Math.Min(maximumBytes, 64 * 1024));
        var block = new byte[16 * 1024];
        while (true)
        {
            var read = await stream.ReadAsync(block, cancellationToken);
            if (read == 0)
            {
                return buffer.ToArray();
            }

            if (buffer.Length + read > maximumBytes)
            {
                throw new ResponseLimitExceededException();
            }

            await buffer.WriteAsync(block.AsMemory(0, read), cancellationToken);
        }
    }

    private sealed class ResponseLimitExceededException : Exception;
}
