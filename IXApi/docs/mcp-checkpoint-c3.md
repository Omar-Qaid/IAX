# C3 catalog compiler checkpoint

Date: 2026-09-17  
Status: implemented and verified; execution remains disabled.

## Delivered

- Loads `manifest.json`, `ixapi.openapi.json`, `operations.json`, and `pilot-contracts.json` as one artifact set.
- Verifies the exact OpenAPI SHA-256 and manifest operation/pilot counts.
- Accepts only format-version 1, discovery-only, non-executable manifests.
- Rejects missing files, invalid JSON, duplicate tool names, anonymous or non-pilot inventory entries, enabled discovery contracts, missing operations, mismatched modules/IDs, unsupported references, missing path/query schemas, and missing output fields.
- Cross-checks the external pilot policy against its OpenAPI `x-ix-pilot-contract` copy.
- Generates immutable typed tool descriptors, restricted JSON input schemas, projected output schemas, and deterministic path/query binding plans.
- Loads all three C1 candidates atomically at startup while publishing zero executable tools.

## Verified catalog

| Tool | Input binding | Output projection | Executable |
|---|---|---|---|
| `finance_customers_search` | approved paging/search query fields | six approved customer fields | No |
| `organization_departments_search` | approved paging/search query fields | five approved department fields | No |
| `workflow_requests_get` | required `id` path field | eight approved request fields | No |

## Verification

The independent Release build succeeds with zero warnings and zero errors. Thirteen tests pass. Fixtures demonstrate that adding an approved endpoint produces a new compiled tool and adding an approved DTO field updates the projected output schema without changing adapter code. Negative tests reject a corrupted OpenAPI hash and drift between OpenAPI metadata and the policy contract.

## Evidence boundary

C3 compiles catalog candidates only. It does not register those candidates with the MCP SDK, execute HTTP requests, forward credentials or company context, project live responses, authenticate users, or make IXMcp ready. `/health/ready` continues returning HTTP 503 because the executable-tool count is zero.
