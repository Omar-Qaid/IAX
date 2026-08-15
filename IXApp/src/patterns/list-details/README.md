# List Details pattern

Implements a master-list/detail layout through `ListDetailsPage`, `ListDetailsLayout`, `TabularDetailPanel`, `useListDetailsPage`, and typed contracts. `d365Tokens.ts` centralizes layout presentation values.

Use when selection in a persistent list drives an adjacent editor/details panel. The feature owns loading, mutation, fields, row identity, and API access; the hook owns selection/page interaction. Do not use for header/line transactions.

[List-details guide](../../../docs/patterns/LIST_DETAILS_PAGE.md) · [Shared grid](../../shared/components/data-grid/README.md)
