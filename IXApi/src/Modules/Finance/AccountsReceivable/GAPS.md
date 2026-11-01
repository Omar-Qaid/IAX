# Accounts receivable — gap analysis against basic D365 F&O

**Last updated:** 2026-11-01
**Scope:** basic (core) D365 F&O Accounts receivable and the sales order cycle only. Credit
management, collections management, commissions, rebates, subscription billing, intercompany sales,
and advanced warehouse shipping are out of scope by request and are not listed as gaps.

## How this file is maintained

Update this file **in the same change** that alters behavior. Closing a gap moves the row out of
*Open gaps* and adds a *Changelog* line. A new deliberate restriction is recorded with the **exact**
message the code raises, so the table stays verifiable by grep. A gap whose standard D365 behavior
cannot be verified goes under *Flagged uncertainties* rather than being guessed at.

## Status summary

| Area | Status |
| --- | --- |
| Customers, customer groups, posting profiles (`CustPostingProfile`, `CustLedgerAccounts`) | Implemented |
| Customer parameters (`CustParameters`) | Implemented |
| Sales orders (`SalesTable`/`SalesLine`): create, quick create, copy, cancel, delivery address, discounts, unit conversion, validation, lookup | Implemented |
| Sales order confirmation | Implemented |
| Packing slip (physical update, physical ledger posting, `CustPackingSlipJour`/`Trans`) | Implemented |
| Sales invoice (financial update, charges, tax) and free text invoice | Implemented |
| Customer payments, payment reversal, settlement reversal, payment methods (`CustPaymMode`) | Implemented |
| Sales returns / credit notes with reason and disposition codes | Implemented |
| Trade agreements / price-discount journals (`PriceDisc*`) | Implemented |
| Customer transactions (`CustTrans`, `CustTransOpen`), sales pools | Implemented |
| Inventory demand and reservation from sales lines | Implemented |
| SQL Server behavior (atomicity, concurrency, idempotency) | **Unverified** — SQLite tests only |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Foreign currency | Sales invoice, free text invoice and credit note posting require the document currency to equal the ledger accounting currency, and the reporting currency to equal the accounting currency. All are rejected with named errors. | A sales order may be in any currency; the invoice posts at the exchange rate on the invoice date, and realized gain/loss is posted at settlement. | Exchange-rate conversion at posting, and exchange-rate gain/loss on settlement. | `SalesInvoiceService`, `FreeTextInvoiceService`, `CustReturnService`, `Foundation/Currency` | **Critical** — core basic AR, and the restriction currently blocks any foreign-currency customer |
| Return dispositions | Only the Credit disposition action is accepted; others are rejected with a named error. | Basic return handling supports Credit, Replace, Scrap and Return to customer dispositions. | Replace, Scrap and Return-to-customer handling. | `CustReturnService`, `ReturnDispositionCodeService` | Medium |
| Inventory dimensions on sales lines | Only site and warehouse are supported; anything else is rejected with a named error. | A sales line may carry any enabled inventory dimension, including batch and serial. | Batch/serial/location dimensions. Consistent with the Inventory module's own basic restriction, so this is a module-wide boundary rather than an AR-only gap. | `SalesInventoryDemandService`, `InventoryDimensionPolicy` | Medium |
| Customer aging | `CustTrans`/`CustTransOpen` carry due dates. | Aging period definitions and a customer aging report/inquiry, plus aging snapshots. | Aging period setup and the report. | `CustTransService`, `CustTransReadService` | **High** — a basic AR expectation and the AP side has the same gap |
| Interest and collection letters | `CustInterest`-related columns exist in migrations; no service. | Basic AR posts interest notes and collection letters against overdue transactions. | Interest calculation and collection letter flows. Collections *management* is out of scope; the interest/collection-letter posting is basic. | `AccountsReceivable` module root | Medium |
| Customer write-off | Not present. | A write-off posts the remaining open balance of a customer transaction to a write-off account. | The write-off flow and its posting profile account. | `CustTrans`, posting profiles | Medium |
| Centralized / partial settlement rules | Settlement and settlement reversal exist, with cash discount. | Basic AR settlement also supports marking specific transactions, settlement priority, and partial settlement with discount recalculation. | Settlement priority configuration; verify partial-settlement discount behavior. | `Settlement/CustSettlementReversalService`, `Foundation/CashDiscount` | Medium |
| Reverse-sign copying | Sales order copy rejects reverse-sign copying, directing the user to the return flow. | A sales order can be copied with reversed signs to create a credit. | Reverse-sign copy. Arguably redundant given the return flow; recorded so the restriction is visible. | `SalesOrderCopyService` | Low |
| Recurring / subscription invoices | Not present. | Out of basic scope (subscription billing is a separate module). | — | — | Low — recorded to mark it as deliberately excluded |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Posting atomicity, concurrent invoice/payment posting, number-sequence contention. | Whole module | **Critical** |

## Deliberate restrictions (verified in code)

| Restriction | Message |
| --- | --- |
| Invoice document currency | `Sales invoice posting requires an order in the ledger accounting currency; foreign-currency conversion is not supported.` |
| Invoice reporting currency | `Sales invoice posting requires the reporting currency to equal the accounting currency; reporting-currency conversion is not supported.` |
| Free text invoice currency | `Free text invoice posting requires the ledger accounting currency; foreign-currency conversion is not supported.` |
| Credit note currency | `Credit note posting requires the return order currency to equal the ledger accounting currency; foreign-currency conversion is not supported.` |
| Return disposition | `Disposition action {action} is outside the basic return scope (Credit and Credit only).` |
| Sales line dimensions | `Only site and warehouse dimensions are supported in the basic inventory scope.` |
| Required dimensions | `Company, site, and warehouse are required for basic inventory dimensions.` |
| Reverse-sign copy | `Reverse-sign copying requires a return-order flow and is not supported for sales orders.` |

## Flagged uncertainties

- **Where interest and collection letters belong.** Microsoft documents them under Credit and
  collections, which is excluded by request, but the posting of an interest note is arguably basic
  AR. Confirm before building.
- **Aging snapshot vs. live aging.** D365 computes aging into a snapshot table for performance.
  Whether a snapshot is wanted here, or a live query suffices, is a local decision.
- **Partial settlement with cash discount.** Settlement reversal and cash discount are implemented;
  whether partial settlement recalculates the discount the way D365 does is not verified.

## Known defect (not a parity gap)

`AccountsReceivableGenericArchitectureTests.Crud_controllers_use_registered_generic_services` fails
because `ICustTransService` is registered through a factory in `FinanceModule.cs`, leaving
`ImplementationType` null for `CustTransController`. This is a registration-shape issue, not a
functional gap, and predates the current inventory work.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-01 | File created from a source scan. Flagged foreign currency as critical, customer aging as high, and recorded the pre-existing `ICustTransService` registration defect. |

## Sources

- [Accounts receivable home page](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/accounts-receivable)
- [Sales orders overview](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/tasks/create-sales-order)
- [Free text invoices](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/create-free-text-invoice-new)
- [Customer payments](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/configure-customer-payments)
- [Sales returns](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/return-order-overview)
