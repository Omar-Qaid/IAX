# ERP example logic and data integrity review

Scope: `Persistence/Seeding/CycleErpExamples`. This review changes seed orchestration and guards only. It does not change accounting services, schema, historical data, seed IDs, versions, or execution policies.

## Confirmed defects and fixes

1. **Duplicate purchase confirmation on retry.** `PurchConfirmationService.ConfirmAsync` creates a new immutable confirmation on every call. The example previously called it whenever its product receipt was absent. A receipt failure after a successful confirmation therefore caused a second confirmation on the next attempt. The example now reuses and validates the existing header and line snapshot, including company, vendor, invoice account, currency, quantity, price, amount, inventory transaction and dimension references. It refuses conflicting snapshots. A receipt with missing confirmation history is reported without recreating history.
2. **Incomplete checks for resumed example data.** The example previously checked selected item/price fields without checking all storage references. Purchase, sales and inventory steps now validate the persisted `InventDim` in DAT against JED/JED-1. They also check header warehouses, relevant unit/price-unit values and invoice accounts. Conflicts fail before subsequent example posting and are preserved.
3. **Incomplete journal/combination conflict checks.** The cash combination lookup now sees soft-deleted records and refuses them instead of attempting another insert. Existing journal lines must retain the expected company, offset company, account types, currency and dimensions before posting resumes.

Focused regression cases cover confirmation replay after an earlier commit, altered/deleted confirmation snapshots, missing confirmation after receipt, other-company confirmations, changed/deleted storage references and shared dimension keys in different companies. They use isolated SQLite databases and do not mock a successful full SQL Server posting cycle.

## Logic traced in existing services

- The scenario is ordered foundation → purchase/receipt/invoice/vendor payment → sale/delivery/invoice/customer payment → inventory draft → general journal. Required setup is declared in the registered dependency graph. The entry remains an opt-in DAT Demo/RunOnce seeder.
- Purchase and sales inventory quantities are 12 PCS and 4 PCS. Persisted dimensions must refer to the example warehouse. The separate +2 adjustment remains a draft; it is not included in the purchase/sale's net +8 inventory movement.
- Invoice taxes, charges, rounding, accounting entries and inventory costs are produced by the existing posting services. Payments use each selected persisted invoice's `InvoiceAmount`; no tax rate or invoice total is guessed by the seeder.
- Payment request keys are distinct for customer and vendor. Existing payments are replayed with their original transaction dates. Payment services validate existing request details and settle the selected invoice rather than unrelated open balances.
- The general journal uses the Bank account type, `DEMO-SAR`, and a Ledger offset. `LedgerJournalPosting` resolves the bank's configured ledger dimension and generates both general-journal entries and `BankAccountTrans`. The seed code does not write posted accounting entries directly.
- Root execution uses the existing distributed startup lock, run-once uniqueness protection and execution history. Relational seeders using the orchestrator context retain atomic data/history commits.

## Limits of confirmation

The connected example's posting services use independent transactions. Earlier stages can remain committed if a later stage fails; stable keys and the corrected confirmation replay permit resumption. Whole-cycle business data and the outer completion-history row are not committed atomically. This existing service-boundary limitation is retained and documented.

No live application database was inspected or seeded. Consequently this review does not certify current database row counts, all historical references, invoice/ledger/bank reconciliation, or a complete clean-install SQL Server run. Those require read-only integrity queries and execution of the example in an isolated SQL Server development database. Passing focused tests and a build must not be described as that end-to-end verification.

## Verification results

- API build passed with zero errors; existing warnings remain.
- All 60 focused seed tests passed, including the new integrity regressions.
- The focused general-journal test for matching vendor/bank subledger movements and accounting entries passed separately.
- Evidence at the repository root: `.artifacts/erp-integrity-review-build.log`, `.artifacts/erp-integrity-review-tests.log`, and `.artifacts/erp-integrity-bank-test.log`.
