# Accounts receivable

## Module purpose

Customers, customer groups, receivables setup, prices and discounts, free text
invoices and sales orders (D365 Accounts receivable / Sales).

## Folders (one per feature)

Each feature folder keeps everything it needs together: `api/` (typed clients),
`pages/` (routed pages), `components/` (feature UI, dialogs, ribbons) and, where
needed, more specific folders.

| Folder | Feature |
| --- | --- |
| `customer/` | Customers (`CustTable`): list, details, open transactions, `CustTrans` inquiry, quick create, customer ribbons. |
| `customer-group/` | Customer groups (`CustGroup`). |
| `free-text-invoice/` | Free text invoices (`CustInvoiceTable`): list and details. |
| `setup/` | Accounts receivable parameters, terms of payment, methods of payment, customer posting profiles. |
| `price-discount/` | Price/discount groups and trade agreement journals (names and lines). |
| `sales-order/` | Sales orders: list, document page, lines grid, journals, dialogs. The reference implementation for document pages, see [sales-order/README.md](sales-order/README.md). |

Shared by the whole module:

- `queries/accountsReceivableQueryKeys.ts`: query-key ownership (company-scoped for sales orders).
- `previews/accountsReceivableRecordPreviews.ts`: D365 record previews (hover cards).
- `routes/accountsReceivableRoutePaths.ts`: route paths.
- `index.ts`: public module exports.

A new feature gets its own folder with the same layout; do not add files to the
module root.

[Modules](../../README.md) · [Data grid](../../../shared/components/data-grid/README.md) · [Master-detail pattern](../../../patterns/master-detail/README.md)
