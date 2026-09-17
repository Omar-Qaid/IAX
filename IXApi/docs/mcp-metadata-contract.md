# C1 metadata contract v1

This contract is generated offline from MVC API Explorer and ASP.NET Core OpenAPI. It is an input to the future IXMcp compiler, not a runtime permission grant. The normal API document is unchanged.

## Operation extensions

| Key | Type | Interpretation |
|---|---|---|
| `x-ix-module` | string | Owning module assembly |
| `x-ix-mcp-exposure` | string | Always `disabled` in C1; no execution admission |
| `x-ix-discovery-status` | string | `pilot-candidate` or `excluded` |
| `x-ix-discovery-reason` | string | Why admission is pending or operation is excluded |
| `x-ix-pilot-contract` | object, optional | Versioned proposed tool identity and restrictions for the three selected canonical read routes |

Generated `operationId` values identify discovery records including aliases. They are not public stable tool names. `pilotContract.toolName` and `contractVersion` are the proposed public identity; changing their meaning requires a versioned contract. `sameActionGroup` in `operations.json` groups actual MVC methods so route aliases can be reviewed without registering duplicate pilot tools.

## Pilot policy

`McpPilotContract` is the code contract for metadata version 1. All entries have `executionEnabled=false`, `method=GET`, and `companyScope=company-required`. These are explicit policy definitions; business DTOs remain generated and are not copied into adapters.

| Tool | API route | Query parameters allowed | Projected fields from APIResponse.data |
|---|---|---|---|
| `organization_departments_search` | `/api/v1/Department/paged` | `PageNumber`, `PageSize`, `SearchTerm` | `recId`, `code`, `name`, `description`, `isActive` |
| `finance_customers_search` | `/api/v1/Customer/paged` | `PageNumber`, `PageSize`, `SearchTerm` | `recId`, `accountNum`, `custGroup`, `currency`, `blocked`, `isActive` |
| `workflow_requests_get` | `/api/v1/WfRequest/{id}` | none | `recId`, `code`, `name`, `processId`, `requestDate`, `isFinished`, `isStopped`, `progress` |

The adapter must reject unknown query parameters, body inputs on these reads, and caller-supplied authentication/company headers. It supplies the validated company context. Search paging defaults to 25 and caps at 100; page numbers must be positive. No includes, nested filters or caller-selected sorting are admitted in this first profile. Path IDs follow the generated schema; Workflow's string ID must also satisfy its existing positive-record lookup behavior, without replacing the backend access check.

Project the listed fields per data item for paged results, or from the single data object for Workflow. Output policy is deny-by-default for all other data fields, including nested audit users, banking, notes and personal information. Copy pagination only through a documented bounded envelope in C3/C4. A field missing from generated schema is a contract-validation failure, not permission to guess a similarly named field. C1 does not perform output projection or claim its execution is tested.

Before admission, required evidence includes adapter input/output-policy enforcement, authenticated company/record isolation, real client invocation, and data-owner review. The candidate schemas are available for C3 without assuming those later gates have passed.

## Authorization representation

The inventory records domain-permission keys from actual filter descriptors and separately records declared authorization policies/role requirements. These are evidence fields, not a fully executable authorization expression. Unknown custom filters/service restrictions keep operations outside the pilot. MVC authorization remains authoritative when real HTTP calls are introduced.

Workflow retrieval calls `CanAccessRequestAsync` before the base lookup. Creator, employee, assignment or broad-view access remains inside IXApi; the adapter must not require broad-view permission for a caller-owned request. NonAction Workflow create/paged/bulk methods must not appear in generated paths.

## Production manifest contract

See [mcp-release-manifest.schema.json](mcp-release-manifest.schema.json). Production promotion will supply an immutable release ID, exact schema-byte SHA-256, policy version, minimum adapter version and supported tool contracts. C7 must validate unique tool-name/version pairs, schema operation correspondence, and compatibility of the routed API pool in addition to JSON schema validation. Source authenticity is supplied by controlled release storage; a digest alone does not authenticate an artifact.

The generated discovery `manifest.json` has `purpose=discovery-only` and `executable=false`; it intentionally cannot serve as a production admission/compatibility manifest. No minimum adapter version is invented before an adapter exists. The discovery exporter does not sign or activate artifacts, and no online refresh endpoint is added in C1.
