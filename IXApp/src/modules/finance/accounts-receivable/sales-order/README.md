# Sales order

The sales order feature (D365 `SalesTable` / `SalesLine`): list, document page
(header, lines, line details), confirmation and packing slip journals. It is the
**reference implementation for document pages**. Copy its structure for new
header-and-lines documents (purchase order, transfer order, free-text invoice…).
See [Document pages](../../../../../docs/patterns/DOCUMENT_PAGE.md).

## Folders

| Folder | What it holds |
| --- | --- |
| `api/` | Typed API clients: `salesOrderListApi` (list, `get` by ID, header update), `salesOrderLinesApi`, confirmation, packing slip, copy. `salesOrderLookupPages.ts` holds server-paged lookup sources (units). |
| `pages/` | Routed pages: `SalesOrderListPage`, `SalesOrderDetailsPage`, `SalesOrderChargesPage`, journals `CustConfirmJournal`, `CustPackingSlipJournal`. |
| `details/` | The document page split by concern (below). |
| `lines/` | The editable lines grid (`SalesOrderLinesGrid`), its cells, line state and discount maths. |
| `components/` | Dialogs: quick create, confirmation, packing slip posting. |

## Document page (`details/`)

`SalesOrderDetailsPage` only wires parts together:

1. `useSalesOrderDocument()` owns state, server data and actions (edit header,
   save, line detail save, address creation, copy, confirm, action pane).
2. `useSalesOrderDocumentData()` holds every query, all keyed by
   `salesOrderQueryKeys` (company-scoped). `invalidateOrder(id)` refreshes the
   whole order and the list after any change.
3. View builders render from the document, each adding parts the next can use:
   `salesOrderTotals` → `SalesOrderHeaderPanel` → `SalesOrderLineFields` →
   `SalesOrderLineTabs` → `SalesOrderHeaderSections`.
4. `salesOrderDetailsConfig.ts` is static configuration: master routes (open
   details), record preview entities, address preview mapping.
5. `useSalesOrderOptions()` returns the translated enum option lists.

## Rules this feature follows

- **Query keys:** only `salesOrderQueryKeys` (company first, then order). No
  hand-written arrays.
- **One record by ID:** the page loads its order with
  `GET /v1/SalesTable/{id}/details`; the record pane reuses the list cache.
- **Master fields:** declare `masterRoute` (open details in view mode) and a
  record preview entity; pass the loaded lookup option so cards show without a
  request.
- **Large lookups:** server-paged (`fetchPage`, `sideMode="server"`), never
  "load all pages".
- **Text:** every label goes through `t('salesOrder.…', 'English default')`, with
  English and Arabic entries in `public/locales`.
- **Edit state:** the page owns Edit/Save/Cancel and passes `editing` to its
  list-details sections.
- **Tests:** `src/test/modules/SalesOrderDetailsPage.test.tsx`,
  `SalesOrderListPage.test.tsx`, `CustConfirmJournal.test.tsx`.

[Accounts receivable](../README.md)
