# Seed data integrity audit — Development ERM database

This audit was run on 2026-10-26 against the configured local Development `ERM` SQL Server database. It is a database snapshot, not a promise that every table should contain seed rows. The complete per-table count is in [seed-table-counts-2026-10-26.csv](seed-table-counts-2026-10-26.csv). Re-run the read-only checks with [seed-data-integrity.sql](seed-data-integrity.sql).

## Coverage

- 242 user tables: 144 populated, 98 empty after the opt-in demo run.
- 29 eligible seeders completed; the latest outcome for every registered seeder ID is `Success`.
- 183 foreign keys; 0 disabled and 0 untrusted.
- Connected example data: 6 customers, 7 workers, 6 products, and 6 workflow requests.
- Checked customer–party, customer–group, worker–party, worker–nationality, product–module, request–process, and request–employee links: 0 violations in each check.

The row counts come from SQL Server partition metadata. They are suitable for coverage and empty-table checks; they are not a transactionally consistent count of a database being written concurrently.

## Empty tables

Many of the 98 empty tables hold operational records created by user actions: sales and purchase orders, invoices, packing slips, inventory transactions and balances, settlements, chat messages, notifications, and journal postings. Inserting arbitrary rows into these tables would bypass the business services that create related ledger and inventory records.

These empty setup areas still need deliberate seed or user-managed setup if the associated workflows must be demonstrated end to end:

- General ledger configuration: `Ledger`, `LedgerChartOfAccounts`, fiscal period links, and `LedgerJournalName` are empty even though `MainAccount` has 21 rows.
- Banking and AR parameters: `BankGroup`, `BankAccountTable`, and `CustParameters` are empty.
- Returns: `ReturnReasonCode` and `ReturnDispositionCode` are empty.
- Inventory master extensions: packaging, barcode, category taxonomy, and inventory status tables are empty. `InventSum` should remain empty until stock transactions occur.
- Organization announcements are empty; they are content records, not a required reference table.

No import-only fixture was enabled. Legacy employee and workflow imports remain opt in and require a source connection or explicit embedded-fixture permission. The demo run used locally generated examples and kept legacy-only process 662 data conditional on that process being present.

## What was corrected during verification

The workflow demo previously assigned a request to fixed employee 157424 and legacy process 662 even when neither existed. It now uses a real demo worker when appropriate and seeds the legacy request and print template only when process 662 exists. The demo request seeder explicitly depends on the worker demo. The full demo run then finished with 29 successful seeders and no failure.

Further end-to-end integrity work should create example orders and postings through application services and assert the resulting customer, inventory, tax, and ledger records together. Empty transactional tables alone are not evidence of a broken seed.
