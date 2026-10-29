# Purchasing blueprint implementation progress - 2026-10-27

User explicitly authorized the full purchasing blueprint, including VAT, accrual accounting, vendor payments, settlement and matching. This expands the previous seed-only task. Existing posted examples and customized records must remain intact.

## Implemented in the first checkpoint

- Added a separate RunOnce `Finance.PurchasePartialInvoices` seeder dependent on the original purchase examples. Creates two distinct orders of 100 PCS at SAR 50: one received/invoiced for 60 with 40 outstanding, and another received/invoiced in 60 then 40 stages. Both use existing services and validate remainders. These examples are currently net-only because purchase VAT is not implemented yet; they are not the completed 15% VAT blueprint.
- New vendor invoice transactions use the purchase order's company-scoped payment term for Net, COD and CurrentMonth due dates. Monthly adds one calendar month; missing or unsupported calendar/schedule/cutoff configuration rejects posting rather than silently assigning invoice date. Previously posted transactions remain unchanged.
- Added due-date boundary tests and an invoice-cycle assertion verifying payment-term due dates on actual posted vendor transactions.

## Purchase VAT and receipt matching checkpoint (2026-10-28)

- Added purchase header/line tax defaults, invoice net/tax/gross and due-date snapshots, input-tax ledger posting and TaxTrans references. Existing posted invoice amounts are preserved by additive migration `20261028065842_PurchaseInvoiceTaxAndReceiptMatching`.
- Added durable invoice-line/receipt-line quantity matches and foreign keys. Invoice posting requires the actual receipt line consumed by the inventory financial update.
- Returns reference their original invoice line, reject excess quantities against that line, and reverse its tax snapshots even after configured rates change. The last credit consumes the remaining rounded tax. Purchase document locks are shared by receipts/invoices/return operations; invoice document uniqueness and tax credits have additional SQL Server application locks. This is not yet a full concurrency certification.
- Added separate RunOnce `Finance.PurchaseVatInvoices` example: 100 PCS at SAR 50 with KSA/FULL input VAT and 60/40 receipts/invoices. It validates SAR 5,000 net, 750 tax and 5,750 gross; it does not repost older examples or replace customized tax rates/accounts. Missing input posting for the standard VAT group uses the existing VAT/prepayments ledger dimension.
- Validation: API build succeeded; 57 focused seed, inventory-cycle, payment-term and table-number tests passed. Includes taxable/non-taxable partial purchase and return cycles, with a tax-rate change before the credit. Migration applied successfully to ERM. Live seed repeatability and reconciliation are being checked separately.
- Current currency guard rejects purchase invoices requiring exchange conversion; exchange-rate accounting is still outstanding. Tax-account changes after an invoice and legacy invoices without tax snapshots need separate policy/coverage.

## Required work still outstanding

Live checkpoint results: both ERM seed runs completed after repairing daily-reset purchase document seed sequences. The new VAT example has exactly one order, two receipts, two invoices and two matches; totals are SAR 5,000 net + 750 VAT = 5,750. All six purchase VAT checks passed. The final focused suite passed 57 tests. Existing cross-module checks found four address violations on the same mixed DAT address; these are not purchase posting failures. User authorized choosing the consistent city, so a narrowly guarded Jeddah repair and rerun test were added. Live application of this address repair is pending: automatic approval review could not complete due to its usage limit. Full purchasing blueprint work remains outstanding as listed below.

1. Extend purchase tax failure/rounding coverage and reconcile every new example against live ledger and tax rows, including preserving original posting accounts on credits.
2. Implement configurable receipt accrual and matched invoice reversal with durable links and correct partial quantities/value. Preserve legacy receipts that never posted accruals.
3. Implement durable receipt-to-invoice quantity matching, price/quantity tolerances and partial match validation. Address locking/races across receipt/invoice/return writers.
4. Add vendor open transactions and settlement relationships, vendor payment posting and journal/API contracts; handle partial/full settlement, residual payment, duplicate request IDs and company/currency/account checks.
5. Finish purchase pricing/discount/charge rules and vendor defaults, including supported addresses and financial dimensions; do not reuse sales-only agreements as purchase rules.
6. Complete return credit tax/accrual behavior, corrections/cancellation/reconfirmation scenarios and supported unit conversions. Ensure source-document ownership on every write.
7. Add schema migrations for required mapped structures; review data-preserving upgrades and rollback behavior. Update EF snapshot/model tests and all impacted test fixtures.
8. Verify full documented 100 @ 50, 15% VAT, 60/40 receipt/invoice, payment and return chain, failure/retry/concurrency tests, source-to-inventory-to-GL reconciliation, idempotent seeds, and fresh-database initialization.

The full blueprint is NOT complete at this checkpoint. Earlier all-table population and integrity counts describe the prior schema, not completion of the new scope.

## Vendor payments and receipt accrual checkpoint (2026-10-28)

- Added vendor open balances, durable settlement links and `POST /api/v1/vendor-payments`. Posting creates the vendor payment, settlement, payment journal, balanced GL voucher and bank movement in one transaction. Supports partial/full settlement and open overpayments; rejects conflicting request reuse, blocked vendors, inactive banks, future invoice settlement and unsupported currency conversion.
- Customer payments now create bank movements too, validate bank/currency/blocked-customer setup, and reject reuse of a payment request for a different customer, amount or bank. Historic customer payments are preserved; bank movements are not fabricated retrospectively.
- Purchase receipt accrual is conditional on the item's `PostPhysicalPurchase` and physical ledger settings. It uses configured physical inventory/accrual posting accounts. Durable receipt snapshots retain original accounts and amounts; matched partial invoices reverse only their portion, with the final portion consuming rounding residuals. Legacy non-accruing receipts stay unchanged.
- Migration `20261028105031_VendorSettlementAndReceiptAccrual` adds the relationships and derives outstanding vendor balances from existing transaction minus settled amounts, excluding deleted/closed transactions. It does not change historic transactions or invent historic settlement links.
- Live payment verification exposed the previously unmapped `BankAccountTrans` entity; a separate additive bank mapping migration addresses that prerequisite. The failed payment transaction rolled back.
- `Finance.PurchaseSettlements` uses the real payment service to settle the dedicated VAT example's two invoices (SAR 5,750 total). It preserves changed settlements and exercises request replay independently of the RunOnce execution log.
- Final validation: 81 focused cycle, architecture, seed dependency and payment-term tests passed. Both reviewed additive migrations were applied to ERM. The purchase payment seeder ran directly twice, followed by the normal seed pipeline twice. The example still has exactly two payment journals totaling SAR 5,750 and two bank movements totaling SAR -5,750; all seven payment/open-balance/settlement/GL checks passed. The additional accrual SQL invariant check is currently vacuous because the live database has no new accruing receipt fixture; accrual behavior is verified by the staged automated test, not claimed as a live scenario.
- Existing inventory full-cycle checks still pass: 115 units (55/60/0), transaction and GL value SAR 6,210, with the existing SAR 0.0001 summary rounding residual. All 14 cross-module checks passed. EF reports no pending model changes. Evidence: `.artifacts/finance-blueprint-final-tests.log`, `.artifacts/finance-payment-seed-replay-final.log`, `.artifacts/finance-blueprint-all-seeds.log`, `.artifacts/purchase-payment-integrity-final.log`, `.artifacts/finance-inventory-regression.log`, `.artifacts/finance-cross-module-regression.log`.

Remaining scope: full foreign-currency valuation/settlement, purchase-specific pricing and discount/charge rules, configurable price/quantity matching tolerances, additional correction/reversal cases, a complete seeded accrual/payment/10-unit-return chain, fresh database initialization, and any requested UI work. The four Markdown blueprints are not yet fully implemented.
