# Architecture boundaries

`scripts/audit-architecture.mjs` defines the intended dependency direction for `src`.

| Source layer | Allowed target layers                                   |
| ------------ | ------------------------------------------------------- |
| `app`        | `app`, `modules`, `patterns`, `shared`, `core`, `mocks` |
| `modules`    | same module, `patterns`, `shared`, `core`, `mocks`      |
| `patterns`   | `patterns`, `shared`, `core`                            |
| `shared`     | `shared`, `core`                                        |
| `core`       | `core`                                                  |
| `mocks`      | `mocks`, `shared`, `core`                               |
| `test`       | every source layer                                      |

The audit also rejects direct imports between top-level modules, imports from the `@mui/icons-material` barrel, circular source dependencies, and unresolved internal imports. Process Builder is explicitly owned by the Workflow bounded context while retaining its established physical package path for route and import compatibility.

## Ownership

- `app`: bootstrap, providers, routes, layouts, shell/navigation, theme, global UI stores.
- `core`: API/auth/error/localization/permission infrastructure and pure generic contracts.
- `shared`: reusable route-agnostic UI, hooks, services, types, and validation.
- `patterns`: reusable page-level compositions with no business-domain ownership.
- `modules`: business pages, components, DTOs, APIs/repositories, queries, schemas, and feature stores.
- `mocks`: shared test/demo datasets; feature-specific adapters may remain with their module.
- `test`: test utilities and suites.

## Current audit state

The architecture audit is expected to pass with no forbidden layer edges, cross-domain imports, icon-barrel imports, circular source dependencies, or unresolved internal imports. Shared enterprise UI tokens live in `shared`; workflow routes are owned by the Workflow module; and dynamic-control contracts are separated from their renderers to keep the component graph acyclic.

Process Builder orchestrates Workflow APIs and is therefore explicitly part of the Workflow bounded context. Its existing `src/modules/process-builder` path remains stable to avoid disruptive route and import churn; `moduleOwnership` in the audit records that ownership narrowly and does not permit other cross-module imports.

## Other audits

`audit:encoding` checks source text. `audit:unused` is advisory because static export analysis can report false positives. See [Development guidelines](development.md) for the full command workflow.
