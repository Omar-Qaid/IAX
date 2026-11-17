# CustFreeInvoiceListPage

The Free text invoice list is implemented at `/accounts-receivable/free-text-invoices` using the same enterprise `SimpleListPage`, DataGrid, action-pane utilities, Options, confirmation dialogs and layout as SalesOrderListPage.

The customer Invoice ribbon opens this list through Related information > Free text invoices, with `customerAccount` in the URL. This is list navigation, not the New > Free text invoice creation action and not an all-invoice journal.

## Available behavior

- Company-scoped list from the existing `GET /api/v1/CustInvoiceTable` endpoint.
- Optional customer context from the URL; customer names and links use the existing customer lookup only when the user can view customers.
- Default Not posted status and Last 30 days, with Posted/All and All dates alternatives.
- Text and invoice-number filtering; shared column filters, selection, refresh, record information and attachments.
- Stored net, sales tax and invoice totals in the shared dialog. No cross-currency aggregation or invented accounting status.
- One draft can be deleted through the existing endpoint after the shared confirmation dialog.
- Posting uses the existing `/api/v1/CustInvoiceTable/{recId}/post` endpoint, with its own permission and explicit confirmation. There is no automatic posting on selection, navigation or opening Totals.
- Delete and Post are unavailable for posted records. The shared list resolves command context from the current visible row, including after filtering or data refresh.

## Scope limits

New/Edit now open CustFreeInvoiceDetailsPage after the user confirmed the header-and-lines screenshot. Posted invoices remain read-only. See free-text-invoice-details.md.
Charges and dedicated Invoice/Accounting/Sales-tax workflows are not implemented by this list. Totals shows the existing tax amount without claiming a tax-transaction inquiry.
Address, one-time customer, accounting status and correction status are omitted because the current free-text invoice API does not return them. The name comes from the customer lookup for Customer account; it is not an invoice-address projection.
The complete posted Invoice journal remains a separate feature.

Frontend permissions match the existing backend resources: `AccountsReceivable.Invoices.View/Create/Edit/Delete/Post`. No security roles, backend posting logic, database schema or application data were changed. The details page adds read-only invoice-authorized lookup routes that reuse the existing lookup service.
