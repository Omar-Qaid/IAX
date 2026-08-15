# Simple-list pattern

`SimpleListPage<T extends { id: string }>` supports standard and enterprise variants and three data sources:

- `static`: provided rows;
- `controlled`: rows plus optional loading/error/refresh;
- `remote`: key, abortable load function, and optional initial rows.

The enterprise configuration adds localized context/view labels, quick or field search, CRUD commands and permissions, utility commands, advanced filtering, related information, initial selection, and panel defaults. The underlying custom `DataGrid` can enable `masterForm` add/edit/save behavior through `onNewRow` and `onRowSave`.

API-backed examples include exchange-rate types and reusable workflow setup lists. Customers and customer groups currently use shared mock datasets. Keep columns memoized, row IDs stable, and mutations in the module API/data-source callbacks.
