# Accounts receivable

## Module purpose

Implements customer/customer-group lists, receivables parameters, payment modes and terms, and sales-order list/document pages.

## Pages and UI patterns

- `CustomerListPage.tsx` and `CustomerGroupListPage.tsx`: list maintenance using shared grid/page controls.
- `CustomerQuickCreate.tsx`: feature-specific customer creation UI.
- `CustParametersPage.tsx`: parameter/setup form.
- `CustPaymModePage.tsx`, `CustPaymTermPage.tsx`: payment setup lists.
- `SalesOrdersPage.tsx`: order list.
- `SalesOrderPage.tsx`: header-and-lines document experience with unsaved-change handling.
- `queries/accountsReceivableQueryKeys.ts`: feature query-key ownership.
- `index.ts`: public module exports.

These pages currently use a mix of shared mock datasets and implemented page logic; do not describe a module API folder that does not exist. New remote integration should introduce module-owned typed API and query hooks while preserving query-key ownership.

[Modules](../../README.md) · [Data grid](../../../shared/components/data-grid/README.md) · [Document pattern](../../../patterns/document/README.md)
