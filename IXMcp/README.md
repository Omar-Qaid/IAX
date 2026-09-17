# IXMcp

Standalone MCP adapter for IXApi. C2 provides the independently buildable host, validated configuration, Streamable HTTP transport, and health gates. It intentionally publishes no executable tools yet.

C3 loads the generated IXApi discovery artifacts, verifies their hash and internal consistency, and compiles approved candidates into typed internal tool and binding descriptors. Discovery artifacts cannot enable execution.

## Run

```powershell
dotnet ../.artifacts/mcp-contract/bin/IXApi/release/IAX.IXApi.dll --export-mcp-contract ../.artifacts/mcp-contract/export
dotnet restore
dotnet run
```

- MCP endpoint: `/mcp`
- Liveness: `/health/live`
- Readiness: `/health/ready`

The configured contract directory must contain `ixapi.openapi.json`, `operations.json`, `pilot-contracts.json`, and `manifest.json` from the same IXApi export. Startup fails with a specific contract error if a file, hash, operation, schema, or policy record is invalid.

Readiness returns HTTP 503 until a later checkpoint admits at least one executable contract. IXMcp communicates with IXApi over HTTP and must not reference IXApi business assemblies or its database context.
