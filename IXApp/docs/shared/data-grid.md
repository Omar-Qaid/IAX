# Data grid

`DataGrid.tsx` exports both `DataGrid` and the compatibility alias `AppDataGrid`. This is a custom grid built with Material UI and TanStack Virtual; MUI X Data Grid is not installed.

## Public model

`DataGridProps<T>` accepts typed `rows`, `ColumnDef<T>[]`, an optional stable `getRowId`, selection callbacks, row commands, display sizing, client/server mode, import/export callbacks, persistence key, and master-form editing callbacks. `DataGridHandle` exposes `startAddRow`, `startEditRow`, `saveEdit`, `cancelEdit`, and `toggleSidebar`.

Columns support width/flex constraints, sorting, filtering, visibility, pinning, custom values/rendering, alignment, simple data types, single-select options, and inline editability.

## Behavior

- Client-side search, filters, and sorting use `useGridDataProcessing`.
- Server mode reports `FetchRowsParams` through `onFetchRows` and supports total/has-more metadata.
- Rows are virtualized on desktop; below `md`, `DataGridMobileBody` renders cards.
- `useGridPersistence` stores versioned column, sort, filter, density, and display preferences when `storageKey` is supplied.
- `masterForm` enables temporary add/edit state and `onRowSave` persistence.
- Sidebar tabs manage columns, filters, and features; they can be hidden.
- CSV export is client-side unless `onServerExport` is supplied.

Keyboard handling includes grid navigation plus shortcuts for search/filter, selection, print, copy, refresh/validate/execute callbacks, and master-form create/edit/save/cancel/delete flows. Treat `DataGrid.tsx` as the exact shortcut reference because availability depends on supplied callbacks and edit mode.

## Usage rules

- Use a stable business ID; never array index.
- Define columns outside render or memoize them.
- Use server mode for server-owned paging/filtering rather than partially filtering a single page locally.
- Surface `loading`, error/retry, empty, and selection states.
- Keep the jsdom virtualization fallback: zero-size test containers otherwise produce no virtual rows.

See the colocated [`src/shared/components/data-grid/README.md`](../../src/shared/components/data-grid/README.md) for lower-level grid notes and [Testing](../testing.md) for test setup.
