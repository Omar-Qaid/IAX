# Purchasing blueprint implementation progress - 2026-10-27

User explicitly authorized the full purchasing blueprint, including VAT, accrual accounting, vendor payments, settlement and matching. This expands the previous seed-only task. Existing posted examples and customized records must remain intact.

## Implemented in the first checkpoint

- Added a separate RunOnce `Finance.PurchasePartialInvoices` seeder dependent on the original purchase examples. Creates two distinct orders of 100 PCS at SAR 50: one received/invoiced for 60 with 40 outstanding, and another received/invoiced in 60 then 40 stages. Both use existing services and validate remainders. These examples are currently net-only because purchase VAT is not implemented yet; they are not the completed 15% VAT blueprint.
- New vendor invoice transactions use the purchase order's company-scoped payment term for Net, COD and CurrentMonth due dates. Monthly adds one calendar month; missing or unsupported calendar/schedule/cutoff configuration rejects posting rather than silently assigning invoice date. Previously posted transactions remain unchanged.
- Added due-date boundary tests and an invoice-cycle assertion verifying payment-term due dates on actual posted vendor transactions.

## Required work still outstanding

1. Persist purchase header/line tax and defaulting snapshots; calculate input tax using configured group/code/rate/account data; include tax in posted invoice/credit snapshots and TaxTrans. Preserve original tax treatment on returns.
2. Implement configurable receipt accrual and matched invoice reversal with durable links and correct partial quantities/value. Preserve legacy receipts that never posted accruals.
3. Implement durable receipt-to-invoice quantity matching, price/quantity tolerances and partial match validation. Address locking/races across receipt/invoice/return writers.
4. Add vendor open transactions and settlement relationships, vendor payment posting and journal/API contracts; handle partial/full settlement, residual payment, duplicate request IDs and company/currency/account checks.
5. Finish purchase pricing/discount/charge rules and vendor defaults, including supported addresses and financial dimensions; do not reuse sales-only agreements as purchase rules.
6. Complete return credit tax/accrual behavior, corrections/cancellation/reconfirmation scenarios and supported unit conversions. Ensure source-document ownership on every write.
7. Add schema migrations for required mapped structures; review data-preserving upgrades and rollback behavior. Update EF snapshot/model tests and all impacted test fixtures.
8. Verify full documented 100 @ 50, 15% VAT, 60/40 receipt/invoice, payment and return chain, failure/retry/concurrency tests, source-to-inventory-to-GL reconciliation, idempotent seeds, and fresh-database initialization.

The full blueprint is NOT complete at this checkpoint. Earlier all-table population and integrity counts describe the prior schema, not completion of the new scope.
