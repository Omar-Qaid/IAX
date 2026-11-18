# Phase 10 implementation status

Phase 10 is **in progress**. A read-only FIFO diagnostic preview is available at `GET /api/v1/inventory-costing/fifo-preview?cutoff=YYYY-MM-DD`. It uses the selected company and groups financially posted journal receipts and issues by item and site. For each issue, it shows receipt allocations, recognized cost, proposed FIFO cost, and only the remaining adjustment (`target - posted - prior adjustments`). The preview never writes `InventTrans`, `InventSettlement`, `InventClosing`, summaries, or ledger entries, and always reports `CanExecute = false`.

The preview reports missing cost, financial receipt coverage, unsupported source/transfer relationships, missing site dimensions, non-FIFO item models, and existing closing history it cannot reconstruct. It is limited to straightforward journal transactions. A two-receipt, one-issue fixture was added for the SAR 1,800 estimated versus SAR 1,700 FIFO example. The focused test command did not finish in this pass; the finance module build passed.

## Required before recalculation or close can execute

- Verify the copied schema's settlement keys, run identity, cost amount signs, settled quantity updates, and close/reversal semantics against an inventory database.
- Complete purchase invoice and sales invoice financial inventory updates. Those source paths are prerequisites for meaningful company-wide FIFO settlement.
- Preserve and verify same-site transfer and original-linked return cost relationships, including propagation when destination stock has already been issued.
- Record original ledger posting accounts and financial dimensions so cost adjustments use historical accounts.
- Implement durable run ownership, atomic or resumable execution, financial coverage checks, close boundaries on every posting path, and reversal of the latest eligible close.
- Test quantity and value reconciliation, partial allocations, currency rounding, concurrency, retries, and rollback on the verified database.

The preview is a diagnostic aid, not an executable close plan. Recalculation, close, boundary enforcement, and reversal remain unavailable.
