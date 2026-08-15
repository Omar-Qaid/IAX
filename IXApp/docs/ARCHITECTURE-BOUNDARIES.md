# Architecture boundaries

`scripts/audit-architecture.mjs` defines the intended dependency direction for `src`.

| Source layer | Allowed target layers |
| --- | --- |
| `app` | `app`, `modules`, `patterns`, `shared`, `core`, `mocks` |
| `modules` | same module, `patterns`, `shared`, `core`, `mocks` |
| `patterns` | `patterns`, `shared`, `core` |
| `shared` | `shared`, `core` |
| `core` | `core` |
| `mocks` | `mocks`, `shared`, `core` |
| `test` | every source layer |

The audit also rejects direct imports between top-level modules, imports from the `@mui/icons-material` barrel, circular source dependencies, and unresolved internal imports.

## Ownership

- `app`: bootstrap, providers, routes, layouts, shell/navigation, theme, global UI stores.
- `core`: API/auth/error/localization/permission infrastructure and pure generic contracts.
- `shared`: reusable route-agnostic UI, hooks, services, types, and validation.
- `patterns`: reusable page-level compositions with no business-domain ownership.
- `modules`: business pages, components, DTOs, APIs/repositories, queries, schemas, and feature stores.
- `mocks`: shared test/demo datasets; feature-specific adapters may remain with their module.
- `test`: test utilities and suites.

## Current audit state

As of this documentation review, `npm run audit:architecture` is not clean. The script reports:

- six upward layer edges, including module-to-app route imports and shared components importing pattern tokens;
- direct Process Builder imports from the Workflow module APIs/components contracts;
- no MUI icon-barrel violations.

The exact file list is produced by the command and may change. These are implementation debts, not allowed architecture. They are documented here because a zero-debt claim would be inaccurate. This documentation update does not alter application imports or weaken the audit.

When fixing a violation, move the narrow contract/token to a lower shared owner or compose the dependency in `app`; do not add a broad exception. Process Builder and Workflow need an explicit ownership decision because the builder currently orchestrates Workflow APIs as a separate top-level module.

## Other audits

`audit:encoding` checks source text. `audit:unused` is advisory because static export analysis can report false positives. See [Development guidelines](development.md) for the full command workflow.
