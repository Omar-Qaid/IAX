# Core layer (`src/core`)

The core layer owns low-level infrastructure and contracts that do not depend on application composition, shared UI, patterns, mocks, or business modules.

## Areas

- `api`: Axios client/configuration, interceptors, query client, `ApiError`, `ApiResponse<T>`, paging/query contracts.
- `auth`: API and mock adapters, `AuthContext`, session storage, JWT helpers, coordinated token renewal, guards, and hooks.
- `configuration`: validated access to Vite runtime values.
- `constants`: application defaults, date formats, and storage keys shared below the app layer.
- `errors`: React error boundary, message mapping, and console reporter adapter.
- `localization`: i18next initialization, English/Arabic language metadata, translation hook.
- `permissions`: permission constants and exact permission/role evaluation.
- `routing`: imperative navigation adapter and route helpers.
- `types`: generic entity, pagination, and selection contracts.
- `utilities`: pure date, enum, formatting, object, string, and validation helpers.

## API behavior

Use the shared `apiClient`; module APIs unwrap the backend envelope and map DTOs to UI records. Interceptors attach the token, correlation ID, and company header and normalize HTTP errors. Query defaults live in `queryClient.ts`. Full details are in [API and state](api-and-state.md).

## Authentication and authorization

Authentication state is React context, not Zustand. The token is held in memory and session storage. Permissions are exact string comparisons with `SystemAdmin` and `*` overrides. See [Authentication](authentication.md).

## Error handling

`ApiError` preserves HTTP status, field validation arrays, trace ID, and original problem details. `mapErrorToMessage` creates a display string. `ErrorBoundary` catches render failures and delegates reporting to `errorReporter`, which currently writes to the console and is designed to be replaced by a telemetry adapter.

## Rules

- Core may import only core files and external packages.
- Do not put app routes, MUI application layouts, module DTOs, or mock selection in core.
- Keep utilities pure unless the folder explicitly owns React infrastructure such as auth context or the error boundary.
