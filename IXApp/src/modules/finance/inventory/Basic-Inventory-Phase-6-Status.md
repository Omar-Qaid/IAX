# Phase 6 implementation status

Phase 6 is **blocked for posting**. The existing counting journal calculated `Counted - current PhysicalInvent` at posting time. Its draft request uses a nonnullable decimal `Counted`, so an omitted count and an explicit zero are indistinguishable. The journal has no verified durable freeze that every physical movement path honors. Posting such a count could incorrectly remove legitimate stock movements.

## Changes in this pass

- Counting journal posting now fails with a validation error until a confirmed count and durable movement freeze are implemented. It does not create inventory or ledger effects.
- Draft counting lines reject duplicate item/dimension combinations and manual variance offsets. Unsupported tracking and location dimensions are rejected.
- The existing inventory architecture test was updated to assert the safe posting block and includes a duplicate-count-line case.

## Requirements before enabling posting

1. Verify the copied schema's intended use of `InventCountJour.Ok`, `InventCountJour.CountDate`, baseline fields, and `InventJournalTable.SystemBlocked` or other counting-control storage. Do not repurpose these fields on assumptions.
2. Persist explicit count confirmation separately from the numeric quantity, so zero is a valid entered count and missing input cannot post.
3. Implement a durable count scope and enforce it in every physical writer, including journal posting and sales delivery. Verify purchase receipts and returns when those paths are connected.
4. Prevent overlapping count scopes and detect intervening physical movements even if their net quantity is zero. A count whose freeze was bypassed must be rejected.
5. Add preparation/generation, count entry, preview, abandonment, posted inquiry, and database-backed acceptance tests. Posting may then calculate the difference from the protected baseline, check reservations, cost the variance, and write balanced ledger entries atomically.

Phase 6 must remain incomplete until these controls and its acceptance cases are verified against the inventory database.
