# Document pattern

Implements header-and-lines pages through `DocumentPage`, `DocumentHeader`, `DocumentLines`, and `DocumentTotals`. `useDocumentPage` coordinates document UI state and `types.ts` defines the contracts.

Use for a transactional record with one header and editable lines/totals. The feature supplies fields, line columns, actions, validation, and API mutations. Loading/error behavior should use shared feedback; unsaved navigation must be handled by the feature.

Existing use: accounts-receivable sales order pages.

[Document guide](../../../docs/patterns/DOCUMENT_PAGE.md) · [Data grid](../../shared/components/data-grid/README.md)
