# API integration and state management

IXApp deliberately uses different tools for different kinds of state.

## HTTP transport

`core/api/apiClient.ts` exports the shared Axios instance. It uses `apiConfig`, a 30-second timeout, JSON headers, and interceptors. The request interceptor:

- obtains a fresh-enough access token;
- adds `Authorization: Bearer ...` when authenticated;
- creates `X-Correlation-ID`;
- adds `X-Company` from the selected company in local storage.

The response interceptor converts backend problem responses to `ApiError`. A `401` clears authentication and emits `session-expired`; a `403` emits `access-denied` without clearing the session.

Frontend APIs consume the backend `ApiResponse<T>` envelope and should reject unsuccessful or empty envelopes. Existing examples include `currencyApi`, `workflowMasterApi`, the workflow-specific APIs, and repository adapters under `modules/administration` and `modules/organization`.

## Server state

TanStack Query is configured once in `core/api/queryClient.ts`:

- five-minute default stale time;
- no refetch on window focus;
- one retry for non-4xx query failures;
- no mutation retry.

Prefer module-owned query-key factories and hooks when a feature has repeated reads/mutations, as in `administration/queries`. Some enterprise page patterns instead accept a remote data-source contract and own loading/mutation orchestration. Do not copy API results into a global Zustand store.

## Form and editing state

- React Hook Form and Zod are used by settings forms and supported shared fields.
- `useEntityForm` is a lighter local-state alternative used by non-RHF forms.
- Enterprise `SimpleListPage` and `ListDetailsPage` own their editing drafts through their pattern hooks/data-source contracts.
- `DataGrid` owns transient inline row editing when `masterForm` is enabled.
- Process Builder uses a feature-owned Zustand store because it edits a multi-entity document across workspaces.

`useUnsavedChanges` currently protects browser reload/close with `beforeunload`. Pattern-specific record switching may add its own confirmation behavior.

## UI and preference state

Application Zustand stores are client state only:

- `useAppStore`: selected company, persisted with `STORAGE_KEYS.COMPANY`.
- `useNavigationStore`: drawers plus persisted favorites, recent pages, and pinned state.
- `usePreferenceStore`: theme, density, contrast, RTL override, navigation layout, colors, font, and zoom.
- `useNotificationStore`: in-memory toast queue rendered by `NotificationProvider`.

Feature-local state should remain local unless multiple distant components must coordinate or persistence is an explicit requirement.

## Environment selection

`core/configuration/environment.ts` reads only `VITE_API_BASE_URL` and `VITE_ENABLE_MOCK_API`; `apiConfig` also reads `VITE_APP_NAME`. Development and production use the API adapter. `.env.test` enables the explicit mock adapter. Mock support is feature-specific, not a universal automatic repository for every module.
