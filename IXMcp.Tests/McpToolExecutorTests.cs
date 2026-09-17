using System.Net;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using IAX.IXMcp.Catalog;
using IAX.IXMcp.Configuration;
using IAX.IXMcp.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Tests;

public sealed class McpToolExecutorTests
{
    [Fact]
    public async Task Binds_exact_route_query_and_protected_headers_then_projects_output()
    {
        CapturedRequest? captured = null;
        var handler = new StubHandler(request =>
        {
            captured = CapturedRequest.From(request);
            return JsonResponse(HttpStatusCode.OK, """
                {
                  "success": true,
                  "data": {
                    "recId": 42,
                    "name": "Visible",
                    "secret": "must not escape"
                  },
                  "pagination": { "pageNumber": 2, "pageSize": 25, "totalRecords": 1, "totalPages": 1, "secret": "remove" }
                }
                """);
        });
        var executor = CreateExecutor(handler, EnabledTool());
        using var activity = new Activity("executor-test").SetIdFormat(ActivityIdFormat.W3C).Start();

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"A/B"},"query":{"PageNumber":2,"SearchTerm":"a b"}}"""),
            new ToolExecutionContext("user-token", "DAT", "corr-123"));

        Assert.False(result.IsError);
        Assert.Equal("https://ixapi.test/api/v1/Items/A%2FB?PageNumber=2&SearchTerm=a%20b", captured!.Uri);
        Assert.Equal("Bearer", captured.AuthorizationScheme);
        Assert.Equal("user-token", captured.AuthorizationParameter);
        Assert.Equal("DAT", captured.Company);
        Assert.Equal("corr-123", captured.CorrelationId);
        Assert.Equal(activity.Id, captured.TraceParent);
        Assert.Equal(42, result.StructuredContent["data"]?["recId"]?.GetValue<long>());
        Assert.Equal("Visible", result.StructuredContent["data"]?["name"]?.GetValue<string>());
        Assert.Null(result.StructuredContent["data"]?["secret"]);
        Assert.Null(result.StructuredContent["pagination"]?["secret"]);
        Assert.Equal("DAT", result.StructuredContent["meta"]?["company"]?.GetValue<string>());
    }

    [Fact]
    public async Task Unknown_argument_is_rejected_without_HTTP_call()
    {
        var handler = new StubHandler(_ => throw new InvalidOperationException("HTTP must not be called."));
        var executor = CreateExecutor(handler, EnabledTool());

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"},"headers":{"Authorization":"attacker"}}"""),
            ValidContext());

        Assert.True(result.IsError);
        Assert.Equal("invalid_arguments", result.Code);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task Disabled_and_unknown_tools_are_rejected_without_HTTP_call()
    {
        var handler = new StubHandler(_ => throw new InvalidOperationException("HTTP must not be called."));
        var disabled = EnabledTool() with { ExecutionEnabled = false };
        var executor = CreateExecutor(handler, disabled);

        var disabledResult = await executor.ExecuteAsync(
            disabled.Name,
            Arguments("{}"),
            ValidContext());
        var unknownResult = await executor.ExecuteAsync(
            "missing",
            Arguments("{}"),
            ValidContext());

        Assert.Equal("tool_disabled", disabledResult.Code);
        Assert.Equal("unknown_tool", unknownResult.Code);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task Compiled_absolute_or_traversal_route_is_rejected()
    {
        var handler = new StubHandler(_ => throw new InvalidOperationException("HTTP must not be called."));
        var unsafeTool = EnabledTool() with
        {
            Path = "/api/../admin",
            BindingParameters = [],
            InputSchema = EmptyInputSchema()
        };
        var executor = CreateExecutor(handler, unsafeTool);

        var result = await executor.ExecuteAsync(unsafeTool.Name, Arguments("{}"), ValidContext());

        Assert.Equal("route_not_allowed", result.Code);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task Forged_header_context_is_rejected_without_HTTP_call()
    {
        var handler = new StubHandler(_ => throw new InvalidOperationException("HTTP must not be called."));
        var executor = CreateExecutor(handler, EnabledTool());

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"}}"""),
            new ToolExecutionContext("token\r\nX-Evil: true", "DAT", "corr"));

        Assert.Equal("invalid_execution_context", result.Code);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task Oversized_input_is_rejected_without_HTTP_call()
    {
        var handler = new StubHandler(_ => throw new InvalidOperationException("HTTP must not be called."));
        var executor = CreateExecutor(handler, EnabledTool(), maximumInputBytes: 1024);
        var value = new string('x', 2000);

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments(JsonSerializer.Serialize(new { path = new { id = value } })),
            ValidContext());

        Assert.Equal("input_too_large", result.Code);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task Success_false_is_a_business_failure()
    {
        var handler = new StubHandler(_ => JsonResponse(
            HttpStatusCode.OK,
            """{"success":false,"message":"Business rule denied the request","data":null}"""));
        var executor = CreateExecutor(handler, EnabledTool());

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"}}"""),
            ValidContext());

        Assert.True(result.IsError);
        Assert.Equal("business_failure", result.Code);
        Assert.Equal("Business rule denied the request", result.StructuredContent["error"]?["message"]?.GetValue<string>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "downstream_unauthorized")]
    [InlineData(HttpStatusCode.Forbidden, "downstream_forbidden")]
    [InlineData(HttpStatusCode.BadRequest, "downstream_validation")]
    public async Task Downstream_status_is_mapped(HttpStatusCode status, string code)
    {
        var handler = new StubHandler(_ => JsonResponse(status, """{"title":"Denied"}"""));
        var executor = CreateExecutor(handler, EnabledTool());

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"}}"""),
            ValidContext());

        Assert.True(result.IsError);
        Assert.Equal(code, result.Code);
        Assert.Equal((int)status, result.StructuredContent["meta"]?["downstreamStatus"]?.GetValue<int>());
    }

    [Fact]
    public async Task No_content_returns_a_schema_stable_success()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var executor = CreateExecutor(handler, EnabledTool());

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"}}"""),
            ValidContext());

        Assert.False(result.IsError);
        Assert.True(result.StructuredContent["ok"]?.GetValue<bool>());
        Assert.Null(result.StructuredContent["data"]);
    }

    [Fact]
    public async Task Oversized_response_is_rejected_without_partial_data()
    {
        var body = "{\"success\":true,\"data\":{\"recId\":1,\"name\":\"" + new string('x', 5000) + "\"}}";
        var handler = new StubHandler(_ => JsonResponse(HttpStatusCode.OK, body));
        var executor = CreateExecutor(handler, EnabledTool(), maximumResponseBytes: 1024);

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"}}"""),
            ValidContext());

        Assert.True(result.IsError);
        Assert.Equal("response_too_large", result.Code);
        Assert.Null(result.StructuredContent["data"]);
    }

    [Fact]
    public async Task Redirect_is_not_treated_as_success_or_followed()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Redirect)
        {
            Headers = { Location = new Uri("https://evil.example/steal") },
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });
        var executor = CreateExecutor(handler, EnabledTool());

        var result = await executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"}}"""),
            ValidContext());

        Assert.Equal("downstream_failure", result.Code);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task Caller_cancellation_is_propagated()
    {
        var handler = new StubHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException();
        });
        var executor = CreateExecutor(handler, EnabledTool());
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => executor.ExecuteAsync(
            "test_items_get",
            Arguments("""{"path":{"id":"1"}}"""),
            ValidContext(),
            cancellation.Token));
    }

    private static McpToolExecutor CreateExecutor(
        StubHandler handler,
        CompiledTool tool,
        int maximumResponseBytes = 1024 * 1024,
        int maximumInputBytes = 256 * 1024)
    {
        var state = new McpCatalogState();
        state.Replace(new CompiledCatalog("hash", "version", [tool]));
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ixapi.test"),
            Timeout = Timeout.InfiniteTimeSpan
        };
        var settings = Options.Create(new IXMcpOptions
        {
            ContractDirectory = "unused",
            IXApiBaseUrl = "https://ixapi.test",
            ApprovedPathPrefix = "/api/",
            CallTimeoutSeconds = 5,
            MaximumInputBytes = maximumInputBytes,
            MaximumResponseBytes = maximumResponseBytes,
            MaximumJsonDepth = 64
        });

        return new McpToolExecutor(
            state,
            new IXApiHttpClient(client),
            new ToolArgumentValidator(),
            settings,
            NullLogger<McpToolExecutor>.Instance);
    }

    private static CompiledTool EnabledTool() => new(
        "test_items_get",
        1,
        "Test",
        "Test_Items_Get",
        "GET",
        "/api/v1/Items/{id}",
        true,
        "company-required",
        new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["path"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["id"] = new JsonObject { ["type"] = "string" }
                    },
                    ["required"] = new JsonArray("id"),
                    ["additionalProperties"] = false
                },
                ["query"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["PageNumber"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["minimum"] = 1,
                            ["maximum"] = 100
                        },
                        ["SearchTerm"] = new JsonObject { ["type"] = "string" }
                    },
                    ["additionalProperties"] = false
                }
            },
            ["required"] = new JsonArray("path"),
            ["additionalProperties"] = false
        },
        new JsonObject(),
        false,
        [
            new BindingParameter("id", "path", true),
            new BindingParameter("PageNumber", "query", false),
            new BindingParameter("SearchTerm", "query", false)
        ],
        ["recId", "name"],
        [],
        "test authorization",
        []);

    private static JsonObject EmptyInputSchema() => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject(),
        ["additionalProperties"] = false
    };

    private static ToolExecutionContext ValidContext() => new("user-token", "DAT", "corr-test");

    private static JsonElement Arguments(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            : this((request, _) => Task.FromResult(handler(request)))
        {
        }

        public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            this.handler = handler;
        }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return handler(request, cancellationToken);
        }
    }

    private sealed record CapturedRequest(
        string Uri,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        string? Company,
        string? CorrelationId,
        string? TraceParent)
    {
        public static CapturedRequest From(HttpRequestMessage request) => new(
            request.RequestUri!.AbsoluteUri,
            request.Headers.Authorization?.Scheme,
            request.Headers.Authorization?.Parameter,
            request.Headers.GetValues("X-Company").Single(),
            request.Headers.GetValues("X-Correlation-ID").Single(),
            request.Headers.TryGetValues("traceparent", out var traceParents) ? traceParents.Single() : null);
    }
}
