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
| Entity field surface | 31 AR entities carry 1,462 fields. 357 are not referenced anywhere outside their own definition, EF configuration and migrations. The 71 belonging to out-of-scope modules have been removed; the rest are standard basic D365 columns kept for parity. | A basic implementation needs only the basic columns, but the fields it does carry should be named and typed as D365 names them. | A decision on the 357 unreferenced standard fields: keep for parity, or trim and accept re-adding them when a cycle needs them. | all AR entities | Low - needs a direction, not a fix |
| Foreign currency | **The sales invoice, free text invoice and credit note all translate** a foreign-currency document into the accounting and reporting currencies at the document date's rate, and refuse to post when no rate is configured. Each voucher's balancing line is derived from its translated lines, so it balances exactly. | A document in any currency posts at its date's rate, and realized gain/loss is posted at settlement. | **Exchange-rate gain/loss on settlement**, which is a separate feature and not started: a payment that settles a foreign-currency invoice at a different rate should post the difference to a gain or loss account. | `CustPaymentService`, `CustSettlementReversalService`, `Foundation/Currency` | **High** |
| Return dispositions | Only the Credit disposition action is accepted; others are rejected with a named error. | Basic return handling supports Credit, Replace, Scrap and Return to customer dispositions. | Replace, Scrap and Return-to-customer handling. | `CustReturnService`, `ReturnDispositionCodeService` | Medium |
| Inventory dimensions on sales lines | Only site and warehouse are supported; anything else is rejected with a named error. | A sales line may carry any enabled inventory dimension, including batch and serial. | Batch/serial/location dimensions. Consistent with the Inventory module's own basic restriction, so this is a module-wide boundary rather than an AR-only gap. | `SalesInventoryDemandService`, `InventoryDimensionPolicy` | Medium |
| Centralized / partial settlement rules | Settlement and settlement reversal exist, with cash discount. | Basic AR settlement also supports marking specific transactions, settlement priority, and partial settlement with discount recalculation. | Settlement priority configuration; verify partial-settlement discount behavior. | `Settlement/CustSettlementReversalService`, `Foundation/CashDiscount` | Medium |
| Reverse-sign copying | Sales order copy rejects reverse-sign copying, directing the user to the return flow. | A sales order can be copied with reversed signs to create a credit. | Reverse-sign copy. Arguably redundant given the return flow; recorded so the restriction is visible. | `SalesOrderCopyService` | Low |
| Recurring / subscription invoices | Not present. | Out of basic scope (subscription billing is a separate module). | — | — | Low — recorded to mark it as deliberately excluded |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Posting atomicity, concurrent invoice/payment posting, number-sequence contention. | Whole module | **Critical** |

## Out of scope (recorded so they are not re-raised as gaps)

| Area | Why it is out of scope |
| --- | --- |
| Customer aging periods and aging inquiry | The deliverable is a report/inquiry plus aging snapshots, and reports, dashboards and analytics are excluded. `CustTrans`/`CustTransOpen` already carry the due dates any later aging view would read. |
| Interest notes and collection letters | Microsoft documents both under Credit and collections; advanced credit management is excluded. |
| Customer write-off | A Credit and collections function in D365, excluded with it. |
| Batch / serial / location dimensions on sales lines | Matches the Inventory module's basic boundary: advanced warehouse dimensions are excluded module-wide. |
| Recurring / subscription invoices | Subscription billing is a separate module, excluded. |
| Reverse-sign sales order copy | The return-order flow already produces the credit; reverse-sign copy is an alternative entry path, not a missing business outcome. |

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

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-02 | Corrected a stale claim: the *Entity field surface* row said no migration had been generated for the 71 removed fields and that the model and database therefore differed. Migration `20261102082226_TrimOutOfScopeEntityFields` does cover them — it carries 171 `DropColumn` calls including `CustTable`, `CustTrans`, `CustInvoiceTable`, `CustInvoiceJour`, `CustInvoiceTrans` and `SalesLine` — and the full chain was applied to a fresh SQL Server database on this date. Row downgraded from Medium to Low. |
| 2026-11-02 | Added an *Out of scope* section and moved six rows into it under the agreed basic scope: customer aging (a report), interest notes and collection letters (credit and collections), customer write-off (credit and collections), batch/serial/location sales line dimensions, recurring/subscription invoices, and reverse-sign order copy. |
| 2026-11-02 | Removed **71 unused entity fields** belonging to modules outside basic scope: credit management (`CredMan*`), Retail (`Retail*`), subscription billing (`SubBill*`), bill of exchange, direct debit, e-invoicing, Project, intercompany, and 7 localization fields (`*_W`). Standard basic fields that are merely unreferenced were **kept** - two of them (`CustTrans.CustExchAdjustmentRealized`, and `InventTrans.MarkingRefInventTransOrigin` in Inventory) turned out to be exactly what a later feature needed. |
| 2026-11-02 | **Fixed the long-standing `Crud_controllers_use_registered_generic_services` failure.** `CustTransService` declared two same-arity constructors, `(IUnitOfWork, ICurrentUserService)` and `(IUnitOfWork, ICompanyExecutionContext)`; neither is a subset of the other, so the container could not choose between them and the service had been registered through a factory, which leaves `ImplementationType` null. The second constructor existed only for one test. It is gone, the registration is the ordinary `AddScoped<ICustTransService, CustTransService>()`, and that test now passes — the suite is green apart from work in flight elsewhere. The architecture test is itself the guard against the factory returning. |
| 2026-11-01 | The free text invoice and the customer credit note now translate a foreign-currency document instead of rejecting it, using the same shared translation and exact balancing as the sales invoice. AR currency translation is complete; only exchange gain/loss at settlement remains. Covered by `A_foreign_currency_credit_note_translates_revenue_and_leaves_cost_in_the_accounting_currency`. |
| 2026-11-01 | The sales invoice's customer line is now derived from the translated lines rather than translated independently, so a foreign-currency voucher balances to the cent and `CustTrans.AmountMst` equals what the ledger recorded. |
| 2026-11-01 | The sales invoice now translates a foreign-currency order instead of rejecting it: transaction amounts stay in the order currency, `AmountMst` / `SalesBalanceMst` / `SumTaxMst` / `InvoiceAmountMst` and the ledger's accounting amounts are translated, and cost of goods stays in the accounting currency because inventory cost already is. Covered end to end by `Sales_invoice_translates_a_foreign_currency_order_into_the_accounting_currency`. |
| 2026-11-01 | Posting now passes the customer scope to inventory posting resolution, so an `InventPosting` rule configured for one customer or customer group is applied instead of silently falling back to the All rule. Fix is in `Common`. |
| 2026-11-01 | File created from a source scan. Flagged foreign currency as critical, customer aging as high, and recorded the pre-existing `ICustTransService` registration defect. |

## Sources

- [Accounts receivable home page](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/accounts-receivable)
- [Sales orders overview](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/tasks/create-sales-order)
- [Free text invoices](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/create-free-text-invoice-new)
- [Customer payments](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/configure-customer-payments)
- [Sales returns](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/return-order-overview)
