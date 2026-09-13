# Lookup components

`src/shared/components/lookups` provides relational selection controls:

- `LookupGrid<T>`: portal-based, positioned, virtualized multi-column popover with search, infinite paging, active-row keyboard navigation, and optional actions.
- `LookupGridField`: standalone or React Hook Form wrapper with optional `fetchById` resolution and permission metadata.
- `LookupField`: configurable field/dialog integration for local arrays or paged server data. `searchable` controls the search box, `sideMode="client"` filters supplied `options` in memory, and `sideMode="server"` calls `fetchPage` with debounced `search`, `pageNumber`, `pageSize`, and an abort signal. Enable `lazyLoading` to fetch and append the next server page near the bottom of the list. Its defaults are `displayMode="select"`, `searchable=true`, `sideMode="server"`, and `lazyLoading=true`. Server mode falls back to supplied local `options` when no server data source is configured.
- `LookupDialog`: full dialog selection view.
- `LookupFilterPanel`, `LookupSearchBar`, and `LookupValueRenderer`: supporting pieces.
- `useLookup`: local lookup state helper.

`FetchPageFn<T>` receives page number, page size, search, and abort signal and returns the lookup page contract defined in `types.ts`. Use a stable, entity-specific query key. Supply `fetchById` when an existing stored ID may not appear in the currently loaded pages.

For a large simple lookup, configure `LookupField` with `sideMode="server"`, an entity-specific `queryKey`, and a `fetchPage` adapter. The adapter owns API parameter naming and maps the API response to `{ data, pageNumber, totalPages, totalRecords }`; the shared hook owns debounce, caching, request cancellation, and page accumulation. The older `onFetchOptions(search)` callback remains supported for non-paged server search.

Lookup permission props disable restricted selection in the field; route/API authorization must still be enforced separately. Use the shared grid lookup for large relational datasets and `AppSelectField` for small fixed option arrays.
