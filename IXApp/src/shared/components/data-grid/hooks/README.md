# DataGrid hooks

Owns reusable grid behavior: `useDataGridState`, `useGridDataProcessing`, `useGridDataSource`, `useGridSelection`, `useInlineEdit`, `useGridLayout`, `useGridPersistence`, `useGridAutosize`, and `useLoadMore`. `index.ts` is the local export boundary.

Client processing and remote data-source modes have different ownership; features must provide stable identifiers and honor the hook contracts. Persistence stores layout/preferences, not business records.

[DataGrid](../README.md) · [API and state](../../../../../docs/api-and-state.md)
