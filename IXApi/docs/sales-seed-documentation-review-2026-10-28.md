# Sales seed comparison with CycleSalesorder.md

Static review of the current seeders and the posting services they call. No database run or new test execution was performed for this review. Existing test results do not establish full blueprint parity.

## Conclusion

Partial alignment. The examples use real sales services for creation, confirmation, delivery, invoice and customer payment. They do not demonstrate the complete documented lifecycle or acceptance matrix. Populated tables alone do not demonstrate those workflows.

| Blueprint requirement | Current seed evidence | Assessment |
| --- | --- | --- |
| Customer/product/setup and company ownership | CUST-100, ITEM-001, dat, JED/JED-1; purchase scenarios provide stock | Basic connected example |
| Create and confirm sales order | SalesScenarioSeeder calls quick-create, line and confirmation services | Covered for three single-line examples |
| Open and partially delivered orders | DEMO-SO-OPEN: 2 units; DEMO-SO-PARTIAL: 3 ordered, 2 delivered | Covered; the partial delivery is not invoiced |
| Full delivery and financial invoice | DEMO-SO-INVOICED: 4 units at SAR 150, all delivered/invoiced | Covered through posting services |
| Revenue, tax, COGS and receivable | SalesInvoiceService writes invoice, ledger, tax, customer/open transactions and financial inventory issue | Service path exercised; exact live totals not rechecked here |
| Complete payment and closure | CommercialScenarioSeeder pays SAR 200 against the specific seeded invoice | Partial settlement only; no zero-balance example |
| Price agreement application | Agreement is posted after the sales examples; those lines supply manual prices and do not set UsePriceAgreement | Agreement data exists but its use on an order is not demonstrated |
| Reservation/unreservation | No reservation service invocation in the sales seed paths | Missing scenario |
| Basic picking | No picking service invocation | Missing optional branch; direct shipping itself is permitted by the blueprint |
| Partial invoices and later completion | Partial order is delivered only; invoiced order is invoiced once in full | Missing scenario |
| Returns and credit notes | No customer-return/credit service calls found in seeding | Missing scenario |
| Cancellation, remainder cancellation, reconfirmation | No corresponding service calls in sales seeders | Missing scenarios |
| Discounts, charges, multiple items, units and failure cases | One item, PCS and fixed price per seeded order | Insufficient acceptance coverage |

## Confirmed differences and defects

1. **Payment terms are not applied to sales invoice due dates.** `SalesInvoiceService` assigns invoice date to the header, `CustTrans` and `CustTransOpen` due dates, even though it carries `order.PaymTerm`. The blueprint requires a due date from payment terms (for example NET30). This is a service defect, not something a seeder should hide by editing posted transactions.
2. **Sales document seed sequences retain the daily-reset defect.** `NumberSequenceSeeder` still defines `CustPackingSlipJour` and `CustInvoiceJour` as cyclic with year-only formats. `SysNumberSequenceService.ApplyResetIfDue` resets cyclic counters daily. New sales posting on another day can regenerate a prior document number. RunOnce examples do not test this condition.
3. **The documented numerical example is absent.** The blueprint uses two lines: 10 at SAR 100 and 5 at SAR 200, net 2,000, tax 300 and total 2,300. Current sales examples use one item at SAR 150. Different illustrative numbers are valid, but they do not prove that documented two-line reconciliation.
4. **Price agreement creation is not price-engine verification.** Posting quantity tiers of 150 and 140 after the orders does not verify tier selection, date validity or quantity-triggered repricing.

## Repeatability limits

SalesScenarioSeeder uses stable CustomerRef/ParmId markers, and CommercialScenarioSeeder uses a stable payment request ID and natural keys. These provide retry protection. Both use a separate DbContext because posting services own transactions, so the whole seeder is not one atomic transaction. The review did not run interruption/recovery, concurrent writers, next-day posting or repeated live seeds.

## Recommended next work

Fix the confirmed due-date and seeded numbering defects with focused tests. Then add separate, repeatable examples for reservation, partial invoice completion, full settlement, returns/credits and cancellation. Use production services and new markers, preserving existing posted records. Add a two-line example matching the blueprint and assert inventory, tax, ledger and remaining customer balances together.

## Implementation follow-up

- SalesInvoiceService now calculates company-scoped payment terms for the invoice header, customer transaction and open balance. Existing posted dates remain untouched; unsupported configured schedules reject new posting.
- Standard seeded sales invoice/packing-slip sequences are monotonic and advanced beyond existing document numbers. Customized formats are preserved.
- Added Finance.SalesCompletionScenarios after CommercialScenarios and PurchaseVatInvoices. Its separate order reserves 10 units at SAR 100, delivers 6/invoices 4, then delivers 4/invoices 6, and pays both invoices. It then receives one returned unit and posts its SAR 115 credit; this credit on a paid invoice is a customer credit balance, not a refund payment.
- Separate cancellation example uses the quantity-tier agreement at SAR 140, confirms, reserves 10, releases 4 and cancels the remaining demand. Stable document/request keys support interrupted-run recovery.
- Live exercise found an oversized payment request reference; the seed key was shortened and the payment service now rejects IDs longer than the persisted field before posting.
- Live exercise also found dimension-specific price agreements were ignored on line creation because the search ran before InventDimId resolution. Line creation now reselects the price after resolving dimensions, preserving manual prices and recalculating supplied discounts.
- Live cancellation exposed another defect: only Ordered transactions were counted, so partially reserved orders could not be cancelled. Cancellation now closes matching Ordered and ReservPhysical demand, releases each summary bucket and recalculates availability. A focused regression checks unchanged physical stock, value and ledger rows.
- Still outstanding: exact documented two-item SAR 2,300 example, optional basic picking implementation, delivery-remainder cancellation, reconfirmation example, comprehensive discount/charge and UOM seed scenarios, plus full concurrency/failure acceptance coverage. This is not full blueprint completion.
- Verified after these changes: 26 focused tests passed; both ERM seed runs completed. All seven checks in `seed-sales-completion-integrity.sql` passed after the final transaction committed. The 14 existing scenario checks also passed (ledger balance, invoice totals, open balances and inventory relationships/quantities). Failed intermediate runs resumed posted documents using stable keys; no historical invoices were rewritten.

## Advanced sales follow-up (2026-10-28)

The outstanding list above describes the earlier checkpoint. `Finance.SalesAdvancedScenarios` now adds:

- Basic picking through `POST api/v1/inventory/reservations/picking`: target quantity in inventory units, partial/full picking, unpicking back to reservation, and repeated-target safety. It validates the sales source, company, dimensions, lifecycle and inventory status. Picking changes allocation buckets without posting physical inventory or ledger movements. Delivery consumes picked stock; availability calculations exclude other orders' picked stock.
- `DEMO-SO-TWOITEM`: ITEM-001 initially 9 PCS, then changed to 10 and reconfirmed; ITM-1002 is 5 PCK. Both confirmation snapshots remain. Final invoice net SAR 2,000, VAT 300, total 2,300.
- `DEMO-SO-DISCOUNT-UOM`: 10% line discounts, an additional SAR 10 multiline discount on the chair line, and 1 BOX converted to 5 inventory PCK. Final invoice net SAR 1,070, VAT 160.50, total 1,230.50. A separate posted purchase supplies four BOX (20 PCK) first.
- Stable order/document markers and checks for existing posted invoices protect reruns. Each line exercises partial picking, unpicking, full picking and a repeated full-pick target before picked-only delivery.

Live retry reached ERM and exposed stale tracked number-sequence snapshots in the preceding atomic inventory example after independent posting contexts had advanced them. That example now starts with a fresh tracker only when no pending changes exist; it does not overwrite custom records or bypass concurrency checks.

Validation: build succeeded; 22 inventory-cycle/dependency tests and 4 on-hand tests passed. The updated runner completed two ERM seed passes. All 8 checks in `seed-sales-advanced-integrity.sql`, 14 in `seed-scenario-integrity.sql`, and 3 in `seed-inventory-completion-integrity.sql` returned zero violations. The sales checks verify exactly one invoice per example, totals, zero remainders, BOX conversion, both confirmation snapshots and financially issued inventory. RunOnce reruns were verified; arbitrary interruption points and concurrent writers were not exhaustively tested.

Remaining beyond this checkpoint: advanced WHS picking routes/UI, delivery-remainder cancellation examples, comprehensive charges/discount combinations and concurrency/failure acceptance coverage. Basic picking uses the existing inventory tables; no new WHS schema was introduced.

Sources: `src/Modules/Finance/AccountsReceivable/CycleSalesorder.md`; `SalesScenarioSeeder.cs`; `SalesAdvancedScenarioSeeder.cs`; `CommercialScenarioSeeder.cs`; `SalesInvoiceService.cs`; `CustPaymentService.cs`; `SalesOrderLineService.cs`; `SalesLineInputDto.cs`; `NumberSequenceSeeder.cs`; `SysNumberSequenceService.cs`.
