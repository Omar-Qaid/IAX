# Administration module

## Purpose and flow

Administration implements application settings and system number-sequence maintenance. Routes resolve to `ApplicationSettingsPage` and `SysNumberSequencePage`; settings use `settingsQueries` → `settingsService` → API or mock repository according to runtime configuration.

## Structure and important files

- `pages/ApplicationSettingsPage.tsx`: settings page composed from global and user forms.
- `pages/SysNumberSequencePage.tsx`: number-sequence list/editing page backed by `sysNumberSequenceApi.ts`.
- `components/GlobalSettingsForm.tsx`, `UserSettingsForm.tsx`: form sections.
- `validation/settingsSchemas.ts`: Zod settings validation.
- `types/settingsTypes.ts`: settings contracts.
- `queries/settingsQueries.ts`, `settingsQueryKeys.ts`: TanStack Query ownership.
- `services/settingsService.ts`: selects `settingsApiRepository` or `settingsMockRepository`.

The module uses shared fields/forms/page primitives and notifications. Add new settings to the types, schema, adapter/API contract, queries, and appropriate form rather than calling Axios from the page.

[Modules](../README.md) · [API and state](../../../docs/api-and-state.md) · [Forms](../../shared/components/forms/README.md)
