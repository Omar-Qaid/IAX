# List Details pattern

Implements a master-list/detail layout through `ListDetailsPage`, `ListDetailsLayout`, `TabularDetailPanel`, `useListDetailsPage`, and typed contracts. Shared enterprise presentation values live in `shared/constants/enterpriseUiTokens.ts` so lower-level primitives and patterns depend in the correct direction.

Use when selection in a persistent list drives an adjacent editor/details panel. The feature owns loading, mutation, fields, row identity, and API access; the hook owns selection/page interaction. Use this pattern for setup records such as customer payment terms, tax codes, tax groups and exchange rates. Use master-details for sales-order documents.

Enterprise pages provide a title/status row, subtitle and Standard view label, collapsed resizable navigation and the shared action pane. Detail sections appear together without automatic Lines/Header tabs. Sales-order documents use the master-details pattern.

Grids use `PatternDataGrid`, shared with master-details and the free-text invoice: 24px rows, 27px headers, bordered cells, table presentation, and initially hidden filters. Custom editors use the same row-height wrapper. Explicit page settings remain supported.

Pattern-owned pages provide a shared edit session. Nested pattern grids register automatically: Save commits their active rows before validating and saving the page, and a line failure keeps the page and grid in Edit mode. Save and Cancel clear row editors when returning to View. Grid pencils request page Edit mode first; line Add/Remove actions require page Edit mode. Saving locks the editors without switching modes. Custom externally controlled pages (`readOnly` with their own actions) retain responsibility for their edit lifecycle.

A section can carry its own command strip (`commands`) and sub-tabs (`tabs`), the way D365 places commands and tabbed panels under a FastTab heading. A group names the tab it belongs to with `tab`; a group without one shows on every tab. The section keeps its own tab selection unless the page passes `activeTabId`.

Action-pane commands keep the order the page declares them in: a command with `menuLabel` becomes a dropdown at the position of its first entry, so a menu sits where the reference form puts it rather than after every button.

[List-details guide](../../../docs/patterns/LIST_DETAILS_PAGE.md) · [Shared grid](../../shared/components/data-grid/README.md)
