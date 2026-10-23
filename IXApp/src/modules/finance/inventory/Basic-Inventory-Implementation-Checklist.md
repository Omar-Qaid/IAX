# Basic inventory implementation checklist

Updated 2026-10-23. The checked-in C# entities and EF configurations are the working model requested by the user. They do **not** establish the deployed SQL Server schema or stored enum values. The connected `ERMOld` database contains workflow tables, not inventory tables. No production migration or cutover has been performed.

| Phase | Current result | Next release gate |
| --- | --- | --- |
| 0 — Baseline | Partial | Compare the copied inventory database with the read-only schema audit and EF model. Verify status enums, keys, financial dimensions, posting links, and company scope. |
| 1 — Configuration | Partial | Verify copied schema, permissions, number sequences, posting profiles, and used-configuration restrictions on SQL Server. |
| 2 — Products | Partial | Verify release, conversions, barcode uniqueness, operation readiness, and company isolation on the target database. |
| 3 — Engine | Partial | Establish physical-reservation enum mapping and source-owned transaction splits; share stock locking and summary rules across every writer. |
| 4 — Journals | Partial | Add validate/preview and posting links; prove opening, movement, adjustment, rollback, and retry on SQL Server. |
| 5 — Transfers | Partial | Verify FIFO cost propagation; implement cross-site financial treatment before allowing cross-site transfer. |
| 6 — Counting | Blocked | Persist explicit count confirmation and durable freeze, enforce it in every physical writer, detect stale counts, then enable posting. |
| 7 — Purchase | Missing | Build purchase documents, receipt and invoice matching, inventory and AP/ledger updates against verified source tables. |
| 8 — Sales | Partial | Implement owned reservations, delivery cost posting, invoice financial issue, matching, AR/ledger, and retries. |
| 9 — Returns | Blocked | Complete original invoice links, entitlement, original cost, physical return, and credit posting. |
| 10 — FIFO | Diagnostic only | Verify settlement/closing schema and posting trace, then implement recalculation, close, propagation, boundaries, and reversal. |
| 11 — Reports/go-live | Partial | Complete historical reports, source/GL reconciliation, migration rehearsal, SQL Server cycle, and business acceptance. |

See `Basic-Inventory-Phase-0-Assessment.md` and the individual `Basic-Inventory-Phase-N-Status.md` files for evidence, files, APIs, tests, and blockers. Do not interpret an entity or CRUD endpoint as a completed phase. The basic release is **not ready for go-live**.

## Latest independent work

- Current on-hand paging and item-group/zero filters; inventory-unit display.
- Physical transaction-to-summary diagnostic with mismatch and unclassified-state output; no repair behavior.
- Separate physical, posted, and adjustment cost fields in transaction details.
- Finance module build: passed with zero errors. Frontend typecheck: passed. Focused SQLite test: added, execution stalled and was interrupted. SQL Server verification: not run.
