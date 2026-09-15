# Simple List pattern

Implements list pages through `SimpleListPage`, `useSimpleListPage`, `useSimpleListDataSource`, and typed contracts. It coordinates actions, filtering/selection, and the shared custom DataGrid.

Use for one primary entity collection. The feature owns row types, columns, query/mutation behavior, permissions, and dialogs. Do not use it for an adjacent detail editor or header/lines document.

`variant="standard"` is the default and uses the same shared layout as enterprise lists such as Workflow Categories: action pane first, then the title and grid. Back, Search, Options, refresh, attachments, and the filter-panel rail are supplied by the pattern. Filter fields come from the page columns; no `enterpriseConfig` is required. Supply page-specific actions through `actionPane`; New, Edit, and Delete are not added automatically. Existing `enterpriseConfig` is supported with the default variant to preserve CRUD permissions, commands, and custom filters. The enterprise variant is retained for compatibility and uses the same layout. Use `recordTableName` when audit and attachment identity differs from the display title.

[Simple-list guide](../../../docs/patterns/SIMPLE_LIST_PAGE.md) · [Data grid](../../shared/components/data-grid/README.md)
