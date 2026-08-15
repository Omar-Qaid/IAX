# Application stores

Contains application-scoped Zustand state:

- `useAppStore.ts`: selected company/application state.
- `useNavigationStore.ts`: sidebar and navigation UI state.
- `usePreferenceStore.ts`: theme, density, language, and other visual preferences with persistence integration.

Server responses belong in TanStack Query, form values in React Hook Form, and feature editing state in the owning module.

[API and state](../../../docs/api-and-state.md) · [Shared services](../../shared/services/README.md)
