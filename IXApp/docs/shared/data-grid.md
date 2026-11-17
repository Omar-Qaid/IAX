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

### Line editing (D365 F&O behaviour)

With `masterForm`, a line opens for editing with Enter, F2, double-click, Alt/Ctrl+E or `startEditRow`, and new lines open first with Alt/Ctrl+N or Insert.

| Action | Result |
| --- | --- |
| Up/Down arrow, or Tab past the first/last field | Saves the line, then opens the next line for editing in the same column. A failed save keeps focus on the current line. |
| Click outside the line | Saves the line. |
| Alt/Ctrl+S, Enter | Saves and ends editing. |
| Esc | Discards the changes. |
| Alt/Ctrl+N while editing | Saves the current line, then opens a new one. |
| Alt+F9, Delete, Ctrl+D | Deletes the selected record(s) through `onDeleteSelected` / `onDelete`. |

A line that was opened but not changed is never sent to `onRowSave` (D365 only writes modified records); new lines are always sent. Keys a control already handled (an open lookup or select list, Alt+Down) are left to that control, and Left/Right follow the visual direction in RTL. F5, F7 and F9 are taken from the browser only when `onRefresh`, `onValidate` or `onExecute` is supplied.

### Lazy loading and scroll control

Desktop rows and mobile cards are both virtualized and share one infinite-scroll hook (`useLoadMore`).

- The next page is requested when the remaining content is shorter than one viewport (or five rows, if larger), so tall mobile cards prefetch in time.
- One page request runs at a time. The page index comes from the rows already loaded (`rows.length / pageSize`), so a failed page is requested again rather than skipped.
- A page that brings no rows (error or empty) is retried only on a later user scroll, after a 2-second back-off, never in a loop. A request that never settles stops blocking after 15 seconds.
- Pass `loading` while any page is in flight, and `hasMore` or `totalRowCount`, so the grid knows when to stop.
- A "Back to top" button appears after scrolling two viewports. `DataGridHandle.scrollToTop()` and `scrollToRow(id)` scroll from code; `scrollToRow` returns `false` for a record that is not loaded.

The column header is outside the scroll area and pinned columns are sticky, so both stay visible while scrolling. Mobile cards have no column header to pin: each card carries its own field labels.

### Record preview

A column referencing master data opts into the D365 hover card with `preview: { entity }` (key = cell value, or `getKey(row)`). See [record-preview.md](record-preview.md).

### Filters

The filter row matches numbers and booleans exactly, dates by prefix, and text by "contains". The column filter offers operators per type: text gets contains/does not contain/begins/ends with, numbers and dates get greater/less than, and all types get equals/not equals/is one of/is blank/is not blank. Blank checks need no value and map to the server's `isempty`/`isnotempty`.

CSV export writes a UTF-8 BOM (so Arabic opens correctly in Excel) and prefixes values starting with `=`, `+`, `-`, `@` with `'`, except plain negative numbers.

## Usage rules

- Use a stable business ID; never array index.
- Define columns outside render or memoize them.
- Use server mode for server-owned paging/filtering rather than partially filtering a single page locally.
- Surface `loading`, error/retry, empty, and selection states.
- Keep the jsdom virtualization fallback: zero-size test containers otherwise produce no virtual rows.

See the colocated [`src/shared/components/data-grid/README.md`](../../src/shared/components/data-grid/README.md) for lower-level grid notes and [Testing](../testing.md) for test setup.
