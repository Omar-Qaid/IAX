# Business modules (`src/modules`)

Modules own domain pages, DTOs/view models, APIs or repository adapters, queries, validation, and feature-specific components. They may compose `patterns`, `shared`, and `core`, but direct imports between top-level business modules are rejected by the architecture audit.

## Current modules

| Module | Implemented pages and behavior |
| --- | --- |
| `identity` | Login form using `AuthContext` and the configured auth adapter. |
| `dashboard` | `WorkspacePage` populated from the shared mock datasets. |
| `finance/accounts-receivable` | Customer and customer-group lists, parameter setup, payment mode/term list-details pages, sales-order list and document view. Several pages currently use module-local state or shared mock datasets rather than HTTP. |
| `finance/foundation` | Currency and exchange-rate-type API-backed pages; exchange-rate details currently use local page data. |
| `administration` | API/mock repository settings forms with TanStack Query; API-backed number-sequence list-details page. |
| `organization` | Legal-entity list-details page using a repository interface selected between API and mock adapters, plus logistics drawers. |
| `workflow` | API-backed process/category/type/control/priority/variable/step/activity setup pages and reusable workflow APIs. |
| `process-builder` | Multi-workspace workflow designer integrating process, activity, request-control, option, validation, and transition APIs. See [Process Builder](process-builder.md). |

## Observed implementation styles

The project does not require every module to contain every possible subfolder. Use only the folders a feature needs:

- `api/` for HTTP DTO mapping and resource operations;
- `adapters/` plus `services/` when API/mock implementations share a repository contract;
- `queries/` for reusable TanStack Query hooks and key factories;
- `components/`, `pages/`, `types/`, and `validation/` as appropriate.

Existing workflow setup pages intentionally share `WorkflowSetupListPage` and `createWorkflowMasterApi`. Enterprise list/detail pages often pass a remote data-source contract directly to their pattern instead of defining a separate query hook.

## Module rules

- Keep DTO conversion and API-envelope handling at the module boundary.
- Do not call raw Axios from visual components; use `apiClient` in an API/repository.
- Do not claim mock support unless the feature selects a real mock adapter or consumes a mock dataset.
- Put feature query keys beside the feature and invalidate the narrowest stable prefix.
- Pages may use a custom layout when a specialized workflow requires it, as Process Builder does; reusable behavior should still be extracted to the appropriate layer.
