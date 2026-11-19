# List Details pattern

Implements a master-list/detail layout through `ListDetailsPage`, `ListDetailsLayout`, `TabularDetailPanel`, `useListDetailsPage`, and typed contracts. Shared enterprise presentation values live in `shared/constants/enterpriseUiTokens.ts` so lower-level primitives and patterns depend in the correct direction.

Use when selection in a persistent list drives an adjacent editor/details panel. The feature owns loading, mutation, fields, row identity, and API access; the hook owns selection/page interaction. Do not use for header/line transactions.

Grids use `PatternDataGrid`, shared with master-details and the free-text invoice: 24px rows, 27px headers, bordered cells, table presentation, and initially hidden filters. Custom editors use the same row-height wrapper. Explicit page settings remain supported. `TabularDetailPanel.onCellEditRequest` lets the owning page enter edit mode from a cell pencil while retaining its permissions and save handlers.

[List-details guide](../../../docs/patterns/LIST_DETAILS_PAGE.md) · [Shared grid](../../shared/components/data-grid/README.md)
