# Inventory blueprint versus current seeds

No file named inventory.md was found. Reviewed the inventory blueprint at `src/Modules/Finance/Inventory/CycleInvnetory.md`, the earlier review, current seeders and relevant posting/closing services. This is a source audit, not a new live database or test run.

## Result: partial alignment

| Area | Current evidence | Result |
| --- | --- | --- |
| Shared product / company released item | Product, module, dimension and posting setup seeders | Basic setup covered |
| Inventory journal receipt/issue | InventoryScenarioSeeder posts ITM-1002 +10/-2 PCK at 20 through services | Covered, different from the blueprint opening +100 at 50 |
| Purchase receipt/invoice | Purchase scenario, partial-invoice and VAT seeders use posting services | Open/partial/full purchase paths covered |
| Sales physical/financial issue | Sales scenario and completion seeders call delivery/invoice services | Partial and completed paths covered |
| Reservation/unreservation | SalesCompletionScenarioSeeder reserves, releases and cancels demand | Now covered; the previous review is outdated here |
| Customer return/credit | SalesCompletionScenarioSeeder receives one unit and posts its credit | Now covered; vendor return is still not invoked by seeders |
| Counting | Draft count of 9 against the journal example's 8 | Not a posted counting scenario; service blocks posting without confirmed count and durable movement freeze |
| Movement/transfer journals | IJ-MOV/IJ-TRF names exist, but no execution example found | Missing scenario coverage |
| Transfer order / transit stock | No seed execution found | Missing scenario coverage |
| Picking, batch/serial transactions, lifecycle failures | Setup does not exercise these operations | Missing transactional examples |
| FIFO close | Same-cost journal example is closed at 2026-10-02 | Does not prove different-cost-layer adjustments or full-cycle close |
| Standard cost | Planned cost records | Activation/revaluation not demonstrated |

## Confirmed mismatch

The blueprint's FIFO example disables financial negative inventory. `InventorySeeder` still creates FIFO with `NegativeFinancial = Yes`. This is a configuration mismatch. Existing customized or used groups should not be overwritten; a dedicated blueprint group/item would allow the strict example without changing other business policy.

## Important service/document differences

The current InventCloseService implementation accepts verified journal, sales and purchase origins and excludes transfers; its class summary still describes journal-only scope. The earlier inventory review's statement that all non-journal origins are rejected is therefore stale. This broader implementation is not exercised by the journal-only close example, so full end-to-end FIFO settlement is not verified by that seed.

InventoryScenarioSeeder's fixed historical cutoff refuses existing financial history through 2026-10-02 to protect business transactions. It is a guarded demo, not a generally rerunnable close operation. RunOnce execution does not by itself demonstrate recovery from every interrupted posting or compatibility with arbitrary existing history.

The blueprint's complete worked example is not reproduced: opening +100, purchase +50, sale -30, transfer journal 20, damage -5, count +3, transfer order 40, customer return +2 and vendor return -5, ending WH-01=55 and WH-02=60 (total 115). Existing seeds use different items/quantities and do not form that one reconciled chain.

## Next implementation order

1. Dedicated strict FIFO example and different-cost layers, preserving existing groups.
2. Movement and transfer-journal examples through supported posting services.
3. Vendor return example and complete quantity/value reconciliation.
4. Implement prerequisites for protected posted counts, transit transfers and picking before claiming seed coverage.
5. Verify the exact documented final quantities and inventory-to-ledger value reconciliation, including reruns.

No business logic or data was changed during this review.

## Implementation follow-up

Added `Finance.InventoryCompletionScenarios`, dependent on the earlier inventory and completed sales examples. It uses production services to post an explicit-offset movement issue of one PCK of ITM-1002, transfer two PCK from JED-1 to a dedicated standard warehouse JED-DEMO within the same site, and return five ITEM-001 units from the VAT purchase invoice. The vendor credit must be SAR -250 net, -37.50 VAT, -287.50 gross. This is a same-site transfer journal, not a transit transfer order. The seeder participates in the orchestrator transaction; its RunOnce policy and stable markers preserve existing posted examples and stop on conflicting unfinished records.

New FIFO setup now explicitly disables physical and financial negative inventory. The seeder's existing insert-only check preserves model groups already present, so this does not retroactively change ERM's existing FIFO policy.

Remaining implementation prerequisites: confirmed count plus durable movement freeze for counting, transit transfer orders, different-cost-layer close/reconciliation and the complete documented 115-unit chain. Basic picking was subsequently implemented and verified by the advanced sales examples (see below). Existing counting/cross-site protections are retained; seeded rows are not substituted for missing posting behavior. Do not report full blueprint completion from these examples.

Live verification exposed daily-reset inventory journal IDs and a missing duplicate-number guard in journal creation. Standard IJRN sequences are now monotonic and advanced beyond existing IDs; creation rejects an already-used number. Version 2 of the completion seeder repairs only its identifiable unposted collision rows before posting; original journal headers/posted lines remain intact. Both subsequent seed runs completed. The movement posted -1 PCK / SAR -20; the same-site transfer posted 2 PCK / SAR 40 with balanced issue/receipt quantity and cost. All three inventory-completion checks and all 14 existing scenario checks passed. Vendor return credit is SAR -287.50 including original VAT.

## Follow-up: implementation boundaries

The advanced sales checkpoint covers basic picking, partial picking, unpicking to reservation, repeated target quantities and picked-only shipment. `InventoryReservationService.Picking.cs` uses existing inventory transactions; this is not a WHS picking-route implementation. The two ERM runs and eight advanced-sales checks were completed in that checkpoint, not rerun for this source review.

Current source inspection confirms three remaining prerequisites:

1. `InventCountJourService` provides generic CRUD only. `InventJournalPostingService` deliberately rejects all Counting journals because confirmed counts and durable movement protection are absent. A posted example requires count capture/confirmation, persisted freeze ownership, enforcement at every stock-changing service, variance calculation under the same stock lock, and release on posting/cancellation. Removing the guard or inserting posted rows is not an implementation.
2. No transfer-order entity/service was found. Existing same-site transfer journals post both legs immediately. Transit transfers require an order with separate shipment/receipt quantities and durable linkage between source, transit and destination transactions, plus partial-operation and duplicate-request handling.
3. `InventCloseService.LoadFifoPoolsAsync` does not silently skip transfer transactions: it rejects them through `sourceSupported`. It scans company-wide financial history through the cutoff and exposes no item filter. A dedicated demo item alone therefore cannot isolate a close from existing company history. Close must handle verified transfer cost relationships and relevant return adjustments before the worked chain can be claimed as settled FIFO.

Proposed acceptance sequence after those prerequisites: dedicated strict FIFO setup without modifying existing groups; opening +100, purchase +50, reserve/pick/sell 30, immediate transfer 20, damage -5, confirmed count +3, ship/receive 40 through transit, customer return +2, vendor return -5. Assert each stage, final warehouse quantities 55 and 60, transit zero, total 115, transaction-level cost settlements and balanced applicable ledger postings. Repeated posting requests must preserve document counts and quantities. Use isolated demo company history for close, or an explicitly implemented and tested close scope; do not close existing business history merely to populate examples.

## Completed backend implementation (2026-10-28)

The prerequisites above are now implemented for basic, same-site inventory:

- Counting journals have persisted `CountingFrozen` and `CountingConfirmed` state. Freeze captures the system quantity; confirmation supplies every line's counted inventory quantity; posting checks the unchanged snapshot and posts the variance. Cancellation releases an unposted count. Stock-changing journal, delivery, purchase receipt and return services check the freeze under a shared stock lock. Frozen journals cannot be edited or deleted through the journal service.
- `InventTransferOrder` and `InventTransferPosting` implement a single-item transfer with independent, partial shipment/receipt operations and stable request identifiers. Standard source/destination and a transit warehouse must share a site. Each operation posts a normal transfer journal atomically with its order progress. Receipts carry the outstanding shipment cost, rather than repricing transit stock at the current average.
- FIFO close validates balanced same-site transfer pairs before excluding those internal movements from financial-site layer consumption. Settlement rows link to their receipt transaction. Vendor returns consume the original purchase layer; customer returns inherit the settled original sales cost. A scoped close records `ScopeItemId`, leaves other items untouched, and allows a later company close without settling the same issues twice. Financial posting checks the applicable close boundary.
- Additive migration `20261028095249_InventoryCountingTransitAndScopedFifo` was applied to ERM. Existing FIFO groups remain unchanged; the example uses its own `DEMOFIFO` group with negative physical/financial inventory disabled.

`Finance.InventoryFullCycle` uses `DEMO-FIFO115` in `FIFO-WH1`, `FIFO-WH2` and `FIFO-TR`. It posts opening 100 at SAR 50, purchase 50 at SAR 60, reserved/picked sale 30, immediate transfer 20, damage 5, confirmed count from 95 to 98, transit shipment/receipt 40, customer return 2 and vendor return 5, then closes only this item. The direct replay test found that a vendor return inherits the purchase name; the original-order lookup now excludes return orders.

Live result: 55 PCS in FIFO-WH1, 60 PCS in FIFO-WH2, zero quantity and value in transit; no remaining reservations/picks. Settled sales cost is SAR 1,500, customer-return cost SAR 100, vendor-return cost SAR 300. Inventory transactions and the inventory ledger reconcile to SAR 6,210.00; the stored four-decimal summary totals SAR 6,210.0001, a sub-cent rounding residual. All 16 checks in `seed-inventory-full-cycle-integrity.sql` returned zero violations after two successful direct executions of the seeder (not merely RunOnce skips).

API operations:

- `POST /api/v1/InventJournal/{journalId}/count/freeze`
- `POST /api/v1/InventJournal/{journalId}/count/confirm` with a JSON object mapping line RecId to counted inventory quantity
- `POST /api/v1/InventJournal/{journalId}/count/cancel`; existing journal posting performs the confirmed variance
- `POST /api/v1/inventory/transfers`, `GET /api/v1/inventory/transfers/{id}`, and `POST .../{id}/ship` / `.../{id}/receive`
- `POST /api/v1/inventory-costing/items/{itemId}/close` with `cutoff`; the existing company-close endpoint remains available

Scope limits: these are backend/API operations, not new frontend screens, advanced WHS work/picking routes, multi-line transfer orders, or cross-site financial reclassification. Full concurrent-writer stress testing, close reversal and alternative costing models remain outside this example.

Final verification: 23 focused inventory-cycle, journal lifecycle and seeding-dependency tests passed, including durable counting protection, rejected stock movement while frozen, partial transit operations, duplicate request handling, and company close after item close without duplicate settlement. The normal complete seeding pipeline subsequently completed twice; the 14 cross-module integrity checks also returned zero violations. EF reported no model changes pending since the migration, and `git diff --check` passed.
