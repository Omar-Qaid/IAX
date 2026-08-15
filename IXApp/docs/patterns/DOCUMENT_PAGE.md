# Document pattern

`DocumentPage` composes:

- `PageHeader` with optional string badge;
- optional `ActionPane` content;
- bordered header and lines surfaces;
- optional totals in a responsive `xs=12`, `sm=6`, `md=4` region;
- optional dialogs.

`DocumentHeader`, `DocumentLines`, and `DocumentTotals` are small reusable wrappers. `patterns/document/useDocumentPage.ts` manages a local document and lines collection; `shared/hooks/useDocumentPage.ts` is a separate legacy async hook.

The routed `SalesOrderPage` uses this presentation with mock sales-order data. Lifecycle mutations are not supplied by the pattern and must be implemented by the module.
