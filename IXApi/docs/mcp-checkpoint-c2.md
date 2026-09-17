# C2 standalone IXMcp host checkpoint

Date: 2026-09-17  
Status: implemented and verified; catalog and execution remain disabled.

## Delivered

- Independent `IXMcp` ASP.NET Core server targeting .NET 9 and pinned to SDK `9.0.202`.
- Official `ModelContextProtocol.AspNetCore` `2.2.0` package with Streamable HTTP mapped at `/mcp`.
- Data-annotation settings validation at startup for the contract directory and IXApi base URL.
- `/health/live` reports process liveness.
- `/health/ready` returns HTTP 503 while the executable catalog is empty.
- Independent `IXMcp.Tests` project with the same SDK pin.
- Architecture gates prove that the server has no IXApi business-assembly, Entity Framework, or project references.

## Verification

The server and tests restore independently with the repository NuGet configuration. The Release build succeeds with zero warnings and zero errors. Seven focused tests cover liveness, empty-catalog readiness, MCP transport routing, catalog health, project references, runtime assembly references, and matching SDK pins.

## Evidence boundary

C2 does not load the C1 OpenAPI artifacts, publish tools, call IXApi, authenticate a user, select a company, execute business operations, or integrate the IXApp assistant. Those behaviors remain gated by later checkpoints. The configured IXApi URL and contract directory are validated configuration values only.
