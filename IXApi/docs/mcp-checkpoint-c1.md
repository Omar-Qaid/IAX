# MCP contract discovery: C0/C1

Date: 2026-09-17. Status: offline API discovery and proposed metadata contract implemented and verified; execution remains disabled.

## Confirmed first client

The user selected an **assistant inside IXApp**. The intended integration is:

```text
IXApp chat UI -> authenticated backend orchestration -> IXMcp -> IXApi endpoints
```

Model credentials and downstream delegation credentials stay server-side. IXMcp remains a standalone OpenAPI-driven adapter. Adding a chat interface is not the same as adding an MCP server: backend orchestration must implement the MCP client and the model/tool-call loop. No browser credential copying, process-wide shared administrator identity or model provider has been selected.

C0 still needs the model provider, orchestration hosting location, identity/delegation mechanism, and deployment arrangement. Those choices do not block offline contract discovery. They do block claiming a working embedded assistant or remote user delegation.

## Implemented discovery boundary

`--export-mcp-contract <directory>` branches before normal IXApi startup. It builds a separate MVC/OpenAPI application, discovers module assemblies, and invokes only the OpenAPI document endpoint in memory. It does not start Kestrel, register module services, initialize the database, or start business workers. Configuration sources are cleared and no credentials are required.

Discovery-only operation IDs and extensions apply exclusively inside the exporter; they do not change the normal HTTP API's document configuration. An existing duplicate-route defect in `MarkupTableController` was removed: literal `MarkupTable` attributes repeated the `[controller]` attributes. All distinct URLs, including `ChargesCode` aliases, remain available. Permission metadata comes from actual MVC filter descriptors and authorization metadata. The shared permission-key method uses the same mapping as execution; it does not grant permission.

Artifacts:

- `ixapi.openapi.json`: generated schemas and operations, with exposure disabled.
- `operations.json`: every discovered route/method, controller/action, same-action grouping, permission declarations, parameters, responses and exclusion reason.
- `pilot-contracts.json`: three versioned proposed read contracts with company scope, allowed query parameters, page limits and projected output fields.
- `manifest.json`: discovery purpose, schema digest, assembly identity and counts. This is not the production compatibility manifest.

Aliases remain separately inventoried. Only the proposed canonical Department, Customer and Workflow read routes are marked `pilot-candidate`; all other routes are excluded. A same-action group identifies routes implemented by the same method, not a proof that all routes have interchangeable semantics.

**No candidate is admitted for execution.** Generated DTO schemas may include banking, personal or audit fields. [The metadata contract](mcp-metadata-contract.md) defines the restricted proposed public tool identities, query parameters and output fields; actual adapter enforcement and staging authorization tests belong to later checkpoints. Inventory permission declarations do not replace runtime authorization or fully describe custom filters and service-level access checks.

## Reproduce

From `IXApi`, using its existing .NET 9 SDK:

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Resolve-Path '..') '.dotnet'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:NUGET_PACKAGES = Join-Path (Resolve-Path '..') '.dotnet/nuget-packages'
dotnet restore Tests/IXApi.Tests.csproj --configfile NuGet.Config -p:NuGetAudit=false --artifacts-path ../.artifacts/mcp-contract
dotnet build Tests/IXApi.Tests.csproj -c Release --no-restore --artifacts-path ../.artifacts/mcp-contract
dotnet test Tests/IXApi.Tests.csproj -c Release --no-build --no-restore --artifacts-path ../.artifacts/mcp-contract --filter 'FullyQualifiedName~McpContractDiscoveryTests|FullyQualifiedName~AuthorizationPolicyTests|FullyQualifiedName~CompanyIsolationTests'
dotnet ../.artifacts/mcp-contract/bin/IXApi/release/IAX.IXApi.dll --export-mcp-contract ../.artifacts/mcp-contract/export
```

Generated files are local artifacts, not source-controlled API snapshots. NuGet audit is disabled only for this restore/build gate; no dependency vulnerability assessment is claimed.

## Completion boundaries

### Verification evidence

- Restore succeeded using the repository NuGet configuration and a workspace-local package cache. Restricted restore initially failed with NU1301/SSL; the authorized retry outside the sandbox succeeded.
- Release build succeeded with 0 errors. The final dependency-rebuilding pass reported 162 warnings in existing areas; the earlier broader pass also included Organization warnings. This is not a warning-free whole-project baseline.
- Focused tests: **30 passed, 0 failed, 0 skipped** across `McpContractDiscoveryTests`, `AuthorizationPolicyTests`, and `CompanyIsolationTests`.
- The actual offline CLI export succeeded: **908 route/method operations**, **3 pilot candidates**, no executable tools. Counts include route aliases: 634 same-action groups, of which 120 have multiple routes.
- Generated schemas contain every proposed pilot output field and allowed query parameter. Tests also verify that excluded Workflow CRUD actions are absent, Customer aliases are outside the pilot, and discovery has no database/business-worker registrations.
- Artifact directory: `../../.artifacts/mcp-contract/export/`. Test result: `../../.artifacts/mcp-contract/test-results/mcp-c1.trx`. These files are generated locally and ignored by Git.

| Module | Generated route/method operations |
|---|---:|
| Administration | 51 |
| Communication | 20 |
| Finance | 506 |
| Identity | 30 |
| Organization | 101 |
| Workflow | 200 |

No authenticated live CRUD, server startup/health, model call, MCP client connection or React UI behavior was tested in this checkpoint. The tests prove offline discovery and the selected existing authorization/company test cases, not a live security certification.

This change implements C1 offline discovery and a versioned metadata proposal. The production release-manifest shape is defined in [mcp-release-manifest.schema.json](mcp-release-manifest.schema.json); producing/promoting an executable release is deferred to C7/C9. Candidate data-owner review and authenticated acceptance remain required before actual admission. C0 is partially decided. C2 (IXMcp host), orchestration, React chat UI and live API execution have not started.

The build initially encountered unused Workflow namespace imports in the two Organization files that were already being edited. Only those imports were removed; after the file changed again during the task, the user explicitly approved removing the reintroduced import from its current contents. Other IDE edits were not restored or overwritten.
