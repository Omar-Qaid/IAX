# Basic inventory implementation checklist

Updated 2026-10-23. The checked-in C# entities and EF configurations are the working model requested by the user. They do **not** establish the deployed SQL Server schema or stored enum values. The connected `ERMOld` database contains workflow tables, not inventory tables. No production migration or cutover has been performed.

The user has deferred missing purchase, vendor invoice, posting-link, and reservation-enum definitions until after inventory core and the existing sales integration work. Those dependencies remain open and are not treated as implemented.

| Phase | Current result | Next release gate |
| --- | --- | --- |
| 0 — Baseline | Partial | Compare the copied inventory database with the read-only schema audit and EF model. Verify status enums, keys, financial dimensions, posting links, and company scope. |
| 1 — Configuration | Partial | Verify copied schema, permissions, number sequences, posting profiles, and used-configuration restrictions on SQL Server. |
| 2 — Products | Partial | Verify release, conversions, barcode uniqueness, operation readiness, and company isolation on the target database. |
| 3 — Engine | Partial | Establish physical-reservation enum mapping and source-owned transaction splits; extend the existing journal/delivery stock lock to every writer and unify summary rules. |
| 4 — Journals | Partial | Add validate/preview and posting links; prove opening, movement, adjustment, rollback, and retry on SQL Server. |
| 5 — Transfers | Partial | Verify FIFO cost propagation; implement cross-site financial treatment before allowing cross-site transfer. Journal and sales delivery currently use matching stock-lock keys, but other writers are not covered. |
| 6 — Counting | Blocked | Persist explicit count confirmation and durable freeze, enforce it in every physical writer, detect stale counts, then enable posting. |
| 7 — Purchase | Missing | Build purchase documents, receipt and invoice matching, inventory and AP/ledger updates against verified source tables. |
| 8 — Sales | Partial | Implement owned reservations, delivery cost posting, invoice financial issue, matching, AR/ledger, and retries. |
| 9 — Returns | Blocked | Complete original invoice links, entitlement, original cost, physical return, and credit posting. |
| 10 — FIFO | Diagnostic only | Verify settlement/closing schema and posting trace, then implement recalculation, close, propagation, boundaries, and reversal. |
| 11 — Reports/go-live | Partial | Complete historical reports, source/GL reconciliation, migration rehearsal, SQL Server cycle, and business acceptance. |

See `Basic-Inventory-Phase-0-Assessment.md` and the individual `Basic-Inventory-Phase-N-Status.md` files for evidence, files, APIs, tests, and blockers. Do not interpret an entity or CRUD endpoint as a completed phase. The basic release is **not ready for go-live**.

## Latest independent work

- The inventory, sales-demand, and site SQLite test fixtures now include the setup entities and transaction behavior required by their production services. The inventory-focused run passed **30/30** on 2026-10-23. This validates those fixture scenarios only, not SQL Server posting or end-to-end workflows.
- Duplicate journal posting now orders prior vouchers after materialization, so its idempotent result works on SQLite as well as SQL Server. Legacy journal conversion handling uses a positive factor or a positive fraction and rejects offsets, consistent with basic unit setup.
- Partial sales delivery now allocates catch-weight quantity once from the original transaction quantity; the old calculation applied the partial ratio twice. This correction still needs a source-document integration test with catch-weight data.
- Basic model-group setup rejects advanced reservation, warehouse, production, and batch controls. Older tests were aligned with the FIFO-only and inventory-unit journal policies.
- Current on-hand paging and item-group/zero filters, with frontend controls and inventory-unit display.
- Physical transaction-to-summary diagnostic with mismatch and unclassified-state output; no repair behavior.
- Separate physical, posted, and adjustment cost fields in transaction details.
- A paged inventory-transaction API with item/source/dimension and inclusive physical/financial date filters. The existing list API remains available.
- A read-only physical as-of quantity API based on dated signed transaction portions, with a focused SQLite fixture added but not yet executed. Historical financial value remains open.
- The current full API test suite passed on 2026-10-23: **285 passed, 0 failed, 2 skipped**. The current frontend typecheck passed; an earlier finance module build passed. SQL Server verification has not run.
