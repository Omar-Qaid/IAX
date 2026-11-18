# Simple List pattern

Implements list pages through `SimpleListPage`, `useSimpleListPage`, `useSimpleListDataSource`, and typed contracts. It coordinates actions, filtering/selection, and the shared custom DataGrid.

Use for one primary entity collection. The feature owns row types, columns, query/mutation behavior, permissions, and dialogs. Do not use it for an adjacent detail editor or header/lines document.

`variant="standard"` is the default and uses the same shared layout as enterprise lists such as Workflow Categories: action pane first, then the title and grid. Back, Search, Options, refresh, attachments, and the filter-panel rail are supplied by the pattern. Filter fields come from the page columns; no `enterpriseConfig` is required. Supply page-specific actions through `actionPane`; New, Edit, and Delete are not added automatically. Existing `enterpriseConfig` is supported with the default variant to preserve CRUD permissions, commands, and custom filters. The enterprise variant is retained for compatibility and uses the same layout. Use `recordTableName` when audit and attachment identity differs from the display title.

## Default pattern for list pages

Every new list page uses `SimpleListPage`. The reference design is the sales order list (`accounts-receivable/sales-order/pages/SalesOrderListPage.tsx`).

**What the pattern supplies** (pages do not repeat it):

- **Action pane:** Back (to the previous page), Edit / New / Delete labels, Options (record info and audit), utilities with labels, refresh and attachments.
- **Header:** breadcrumb (the title) and "Standard view".
- **Grid:** the Filter box, with search fields defaulting to every column.
- **Right rail:** the filter pane with its labels. A condition matches with `getValue` when no `matches` is given.
- **Related information:** the panel title.
- **Language:** the current locale.

**What a page sets** in `enterpriseConfig`: only what is specific to it.

- **Data and grid:** `columns`, `dataSource`, `dataGridProps.storageKey`.
- **Record and search:** `recordTableName`, `getAuditRecordId`, `searchFields`, `showSearchCommand`.
- **Actions:** `crud` handlers and permissions (`onEdit`, `onNew`, `onDelete`, `canEdit`, `canDelete`), and `commands` / `getCommands`.
- **Right rail:** `advancedFilter.fields` / `getValue`, and `relatedInformation.sections` using `ListInfoValue`.
- **Context:** `contextLabel`, for example "Sales orders | CUST-100".

**One look for every list:** the pattern sets the grid sizes (31px rows, list header height) and the toolbar: a Filter box with 5px above it, plus the row count. Pages do not set `rowHeight`, `headerHeight` or toolbar styles in `dataGridProps`, so all lists look like the sales order list. Use `filterBar` only for extra business filters.

**Customising one list:** any default can be overridden in that page's config: a label, `backCommand`, `utilities`, `advancedFilter.matches`, `locale`. Layout extension points are props: `actionPane`, `actionPaneEndActions`, `filterBar`, `sidePanels`, `belowGridContent`, `dialogs`, `dataGridProps`, `containerSx` and `contentSx`. Overrides stay in the page, so the shared pattern and other lists are unchanged.

[Simple-list guide](../../../docs/patterns/SIMPLE_LIST_PAGE.md) · [Data grid](../../shared/components/data-grid/README.md)
