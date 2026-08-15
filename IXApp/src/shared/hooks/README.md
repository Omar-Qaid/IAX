# Shared hooks

Generic hooks cover debounce, disclosure, previous value, local storage, notifications, page mode/refresh, list/document helpers, lookup/logistics interaction, and unsaved-change browser handling.

Hooks do not own business endpoints. `useUnsavedChanges` currently attaches `beforeunload`; consumers needing in-app navigation blocking must verify and implement that requirement separately.

[Hooks guide](../../../docs/shared/hooks.md) · [API and state](../../../docs/api-and-state.md)
