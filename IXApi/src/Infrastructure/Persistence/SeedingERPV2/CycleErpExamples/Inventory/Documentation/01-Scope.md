# 01 — Scope

This package demonstrates the **basic** Inventory cycle of `Read_only.md` on real data, using the same split the
Accounts Payable package uses: the module owns every master write, and this package only prepares, executes and
reconciles.

## In scope

- Required configuration: unit of measure, item model groups, item group, inventory posting profiles, site,
  warehouse, the default inventory dimension, inventory parameters and the close number sequences.
- Master data: the fiscal-year-scoped stocked and service items, their dimension groups and order settings.
- Basic transactions through their owning services: purchase receipt and vendor invoice, reservation, sales
  packing slip and sales invoice, and the periodic weighted-average inventory close.
- Reconciliation of the posted result: on-hand quantity, on-hand summary consistency, financial inventory value,
  cost of goods sold, close adjustment, cost settlements, reservations, non-stocked behavior, movement traceability
  and ledger balance.

## Out of scope

- Adjustment, movement, counting and transfer journals, and transfer orders. The services exist
  (`IInventJournalPostingService`, `IInventTransferOrderService`), but their prerequisites do not: see
  `PHASE-TWO.md` for the exact setup that is missing and why it must be seeded before a scenario can claim them.
- Batch and serial tracking, physical-value costing, and every other reference case not listed in
  `Data/acceptance-status.json` as verified by this package.
- Advanced warehouse management, production, master planning and reporting layouts.

## What a successful run does and does not prove

A run proves the figures its scenario asserts, nothing more. `Data/acceptance-status.json` is the authority on
which reference case ids this package certifies; a case not named there is not certified by a green run here, and
`InventoryTransactionFlow.Scenarios` lists only the scenarios that execute end to end through real services.
