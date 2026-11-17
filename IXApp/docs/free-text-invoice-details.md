# CustFreeInvoiceDetailsPage

Routes: `/accounts-receivable/free-text-invoices/new` and `/accounts-receivable/free-text-invoices/:invoiceRecId`. The list New action preserves an optional customer context; invoice links open details and Edit opens an unposted document for editing.

The page uses the enterprise `ListDetailsPage` used by `SalesOrderDetailsPage`: shared action pane and CRUD commands, record-list toggle, Options and utility rail, section layout, Lines/Header tabs, DataGrid line editing, confirmation dialogs, document totals drawer, attachments for saved records, and unsaved-change protection. The shared section configuration supports externally managed document editing so lookup fields retain the correct edit behavior.

## Basic supported flow

Select a customer, review the invoice account, currency, payment terms/method and sales tax group, and enter one or more lines with a description, ledger account, quantity, unit price and tax groups. Defaults come from the selected customer or the existing backend service. Posting profile can be supplied; an empty profile lets the server use the configured customer parameters.

Save commits the active grid editor before sending one whole-document request to the existing create/update endpoint. Server validation, dimension validation, amount/tax calculations and due-date calculation remain authoritative. Changing a header or line does not post it. Unsaved amounts are not shown as calculated server totals.

Post requires its own permission, a saved draft and confirmation. Posted documents disable Edit/Delete/Post. Deletion of a saved draft requires confirmation. Cancel discards local changes. Queries and editor instances are scoped by company and record.

Invoice-authorized lookup routes `/api/v1/CustInvoiceTable/tax-groups` and `/ledger-dimensions` delegate to the existing shared `ISalesOrderLookupService`. Users do not need a SalesOrders permission for these invoice lookups. Header reference lookups use existing Currency, PaymTerm, CustPaymMode and TaxGroup endpoints.

## Scope limits

The API does not expose invoice addresses, one-time customer flags, accounting/correction status, payment schedules, bank accounts, cash discounts, fixed assets, reasons or foreign-trade fields. These were not invented for the screenshot. ZATCA and withholding tax remain outside basic scope. The selected line's ledger/default dimension references can be reviewed; a new financial-dimension set editor is not implemented. Charges, accounting distributions and dedicated tax-adjustment workflows remain separate gaps.

The existing backend replaces the line collection on update, so returned line RecIds are authoritative. No schema, posting rules, tax rules, number sequences or application data were changed.

Reference: [Microsoft Learn — Create a free text invoice](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/create-free-text-invoice-new).

## Validation

- Frontend production build (TypeScript + Vite): passed.
- Invoice/contact focused tests: 15 passed. Shared grid lookup tests: 14 passed.
- Desktop/mobile invoice list and details browser flows: 4 passed; details rerun after dimension-preservation correction: 2 passed. Browser requests use fixtures, not the live invoice database.
- API build: passed with 0 errors.
- Full API regression run from the standard isolated Tests/bin folder: 1,132 passed, 0 failed, 21 SQL Server opt-in tests skipped.
- Existing zero-valued header/line dimensions remain explicit on update; new lines inherit defaults through the server.
- Separate InventoryCycle.Tests regression project: 147 passed, 0 failed, 0 skipped.
