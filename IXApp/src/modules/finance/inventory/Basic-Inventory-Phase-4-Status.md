# Phase 4 implementation status

Phase 4 is **in progress**. Existing inventory journal header, line, posting, inventory transaction, summary, and ledger code supplies a starting point. This pass tightened that path for movement and adjustment journals.

## Implemented in this pass

- Draft lines must use the item's inventory unit. Positive movement and adjustment lines require an explicit positive receipt cost. Adjustment lines cannot supply an offset account; movement lines may inherit the journal name's default offset and must reference a valid account dimension.
- Posting now returns the existing posted journal and vouchers on a retry, without creating another inventory movement.
- SQL Server posting takes transaction-scoped locks for the journal and affected item/dimension keys in a stable order before checking stock or updating summaries. Posting remains all-or-nothing within the existing unit-of-work transaction.
- Issues check `PhysicalInvent - ReservPhysical`, so they cannot consume stock shown as reserved in `InventSum`.
- FIFO issues use a provisional running-average cost from current same-site inventory quantity and value. Transaction and voucher references remain available for the later close and settlement phase. This is an estimate, **not final FIFO settlement**.

## Open work and limits

- Phase 3 physical reservation, transaction splits, shared stock locking across sales and journal paths, and summary reconciliation remain incomplete. Journal locks protect concurrent calls using this journal posting service; other inventory writers do not yet share those locks.
- No authoritative inventory database/schema has been verified. Database-backed acceptance cases, period-close behavior, and posting reference reconciliation still need the copied inventory schema and a test database.
- Posting still writes ledger entities directly in `InventJournalPostingService`; it has not yet been moved to a shared ledger posting service. `InventTransPosting` and the full posted-result trace require verified mappings.
- Journal validate and posting preview APIs, correction references, opening-batch deduplication, and FIFO close remain to be implemented.
- Counting and transfer journals are existing paths; their Phase 5 and 6 acceptance cases are outside this Phase 4 pass.

Phase 4 must not be marked complete until the opening, movement, adjustment, retry, concurrency, rollback, costing, and reconciliation cases pass against the verified inventory database.
