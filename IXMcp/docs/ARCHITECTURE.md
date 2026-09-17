# IXMcp architecture

IXMcp is an independent ASP.NET Core MCP adapter. It communicates with IXApi through HTTP and has no reference to IXApi modules, infrastructure, Entity Framework, or the business database.

## C2 boundary

- `Program.cs` owns hosting, validated settings, health endpoints, and the official MCP Streamable HTTP transport.
- `Catalog` exposes only catalog lifecycle state. Its C2 implementation is deliberately empty.
- `Health` refuses readiness until a validated catalog publishes at least one executable tool.
- `/mcp` is mapped by `ModelContextProtocol.AspNetCore` 2.2.0, but C2 registers no tools.

Contract loading, OpenAPI compilation, execution, delegated identity, and IXApp orchestration belong to later checkpoints.

## C3 catalog compiler

At startup, `McpCatalogLoader` resolves the configured artifact directory and asks `McpCatalogCompiler` to compile a complete replacement catalog. The compiler verifies the OpenAPI SHA-256, manifest counts and safety flags, unique tool names, inventory correspondence, OpenAPI operation metadata, allowed parameters, response fields, local schema references, and duplicated pilot-policy metadata.

The resulting catalog contains immutable tool identities, restricted input and projected output schemas, and deterministic path/query binding plans. It stays internal and non-executable. Invalid input fails startup; the server does not retain or expose a partially compiled catalog.

## C4 HTTP executor

`McpToolExecutor` resolves one immutable catalog snapshot, requires an admitted tool, validates arguments against its restricted schema, and binds only compiled path/query parameters. `ToolExecutionContext` carries credential and company values supplied by a trusted caller; these values cannot be supplied as tool arguments. C5 will establish how that trusted context is authenticated and delegated.

`IXApiHttpClient` has one configured base origin, disables redirects, and accepts only relative routes under the configured `/api/` prefix. The executor applies finite input, decompressed-response, JSON-depth, and timeout limits. It normalizes REST failures, `APIResponse.Success=false`, 204 responses and successful envelopes into one structured result. Returned data and pagination are projected through explicit allowlists.

C4 supports the current read-only JSON pilot profile. Request bodies and writes require compiled writable-field policies and remain excluded.
