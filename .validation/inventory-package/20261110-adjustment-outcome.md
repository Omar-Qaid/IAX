# Inventory adjustment acceptance — 2026-11-10

Focused run: **8 passed, 0 failed, 0 skipped** in 1m43s, excluding build. Evidence: `package-20261110-adjustment.trx` and its log. The run includes master preparation/rollback/conflict/counter checks, reference cycle and repeat, physical-value on/off, and the new adjustment scenario.

ADJ-01 ran in a new SQLite database using the real ApplicationDbContext model with foreign keys and uniqueness retained. Company `dat`, fiscal year 2026, item `IV2-2026-ADJ`, warehouse `APV2-WH`, site `APV2-SITE`.

- Journal `IV2JR-000001`, voucher `IV2JV-000001`: expected/actual +10 units and +100 inventory value. Inventory profit account 481004 credited 100.
- Journal `IV2JR-000002`, voucher `IV2JV-000002`: expected/actual -2 units and -20 inventory value. Inventory loss account 681006 debited 20.
- Final expected/actual quantity 8, value 80, COGS 0. Two movements, two balanced vouchers, no settlements.
- Same-database replay preserved movement/voucher counts and voucher sequence progress. A deliberately changed posted line was rejected without more movements or vouchers.

The reconciliation previously classified every issue as COGS. Scenario 02 requires journal loss to remain separate; reconciliation now selects sales-origin issues for COGS, and both the original sale scenario and adjustment passed.

Following this run, three stage case labels were corrected to match Read_only.md: pre-close running cost CST-01 (not recalculation CST-03), purchase RCV-01 (not nonexistent RCP-01), delivery ISS-01 (not invoicing ISS-02). Historical TRX files retain their original labels. Broader regression on the final source is a separate run.

Full acceptance remains incomplete, including counting, transfers, returns/corrections, batch/serial and concurrency coverage. This is not a completion certificate and the recurring task remains active.
