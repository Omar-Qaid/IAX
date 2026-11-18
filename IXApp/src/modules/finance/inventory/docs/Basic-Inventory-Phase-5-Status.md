# Phase 5 implementation status

Phase 5 is **in progress**. The existing transfer journal already creates paired negative and positive `InventTrans` rows, updates both `InventSum` rows, retains source and destination inventory transaction IDs on its journal line, and posts in one unit-of-work transaction.

## Implemented in this pass

- Same-site transfer draft lines require a positive quantity in the inventory unit, different source and destination warehouses, and only site/warehouse dimensions. Manual transfer cost and ledger offset are rejected.
- Cross-site transfer is rejected at draft entry and rechecked at posting until financial reclassification, account resolution, and cost propagation are implemented.
- Posting validates the open fiscal period for transfers, not just journals that create a ledger voucher.
- All transfer source quantities are checked against **initial** unreserved physical stock by item and source dimension before any line posts. A receipt from another line in the same journal cannot fund the outgoing transfer. The posting path also rechecks availability as each line runs.
- Existing SQL Server journal and item/dimension locks, transaction rollback, and retry-safe posted result from Phase 4 apply to transfers.
- A focused test case was added for multiple outgoing lines exceeding stock without creating inventory transactions. The existing retry assertion now expects the posted result and no duplicate transactions. The focused test command did not finish in this pass.

## Open work and limits

- The paired journal line IDs provide an inquiry path, but the copied schema's transfer origin links and later cost-adjustment propagation are not verified. The transfer is not yet safe to treat as a completed FIFO costing relationship.
- Cross-site financial transfer and inter-unit postings are not implemented, so cross-site requests remain blocked.
- The Phase 3 shared reservation engine and stock lock contract across every inventory writer remain incomplete. Journal locks protect other requests using this journal posting path, but sales and other inventory writers do not yet participate in the same lock scheme.
- Database-backed concurrent posting, rollback, source/destination value reconciliation, and authenticated API checks need the verified inventory database.

Do not mark Phase 5 complete until these remaining acceptance cases pass.
