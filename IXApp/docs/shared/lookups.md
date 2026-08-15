# Lookup components

`src/shared/components/lookups` provides relational selection controls:

- `LookupGrid<T>`: portal-based, positioned, virtualized multi-column popover with search, infinite paging, active-row keyboard navigation, and optional actions.
- `LookupGridField`: standalone or React Hook Form wrapper with optional `fetchById` resolution and permission metadata.
- `LookupField`: simpler field/dialog integration.
- `LookupDialog`: full dialog selection view.
- `LookupFilterPanel`, `LookupSearchBar`, and `LookupValueRenderer`: supporting pieces.
- `useLookup`: local lookup state helper.

`FetchPageFn<T>` receives page number, page size, search, and abort signal and returns the lookup page contract defined in `types.ts`. Use a stable, entity-specific query key. Supply `fetchById` when an existing stored ID may not appear in the currently loaded pages.

Lookup permission props disable restricted selection in the field; route/API authorization must still be enforced separately. Use the shared grid lookup for large relational datasets and `AppSelectField` for small fixed option arrays.
