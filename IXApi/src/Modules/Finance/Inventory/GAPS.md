# Inventory — gap analysis against basic D365 F&O

**Last updated:** 2026-11-02
**Scope:** basic (core) D365 F&O Inventory and Cost management only. Advanced warehouse (WHS),
production, quality, and optional features are out of scope by request and are not listed as gaps.

## How this file is maintained

This file is part of the module, not a report about it. Update it **in the same change** that
alters behavior:

- Closing a gap: move the row out of *Open gaps*, state what now exists, add a *Changelog* line.
- Adding a deliberate restriction: add it to *Deliberate restrictions* with the **exact** message
  the code raises, so the table can be verified by grep.
- Finding a new gap: add a row with verified D365 behavior and a source. If the standard behavior
  cannot be verified, put it under *Flagged uncertainties* instead of guessing.
- Never record a gap that the code does not actually have. Every row here is expected to be
  checkable against source.

## Status summary

| Area | Status |
| --- | --- |
| Dimensions (`InventDim`), transactions (`InventTransOrigin`, `InventTrans`), on-hand (`InventSum`) | Implemented |
| Inventory journals: movement, adjustment, transfer, counting (incl. line generation, summary posting) | Implemented |
| Transfer orders (`InventTransferTable`/`Line`) | Basic transaction lifecycle implemented; SQL Server behavior unverified |
| Cost settlement: FIFO, LIFO, weighted average | Implemented |
| Inventory close, recalculation, close cancellation | Implemented |
| Inventory marking (`InventTrans.MarkingRefInventTransOrigin`) | Implemented |
| Physical and financial inventory value, inventory value report | Implemented |
| Ledger integration (`InventPosting` profiles, subledger vouchers) | Implemented |
| SQL Server behavior (atomicity, concurrency, idempotency) | **Unverified** — SQLite tests only |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Inventory models | FIFO, LIFO and weighted average are accepted at model-group setup and settled by inventory close. Standard cost, moving average, LIFO-date and weighted-average-date are rejected with a named error. | All of these are basic inventory models, each with its own valuation and close behavior. | The four rejected models, and with them standard-cost revaluation and moving-average maintenance. | `InventModelGroupService.SettledModels`, `InventCloseService.SupportedModels`, `InventJournalPostingService.ResolveUnitCostAsync` | Medium — rejected explicitly, not approximated, so nothing is silently wrong |
| Standard-cost variance posting | `InventJournalPostingService` rejects a receipt priced differently from the active standard cost. Unreachable in practice, because standard cost is rejected at setup. | Purchase price variance posts to a variance account per the posting profile. | Variance calculation and posting. Blocked behind the standard-cost model itself. | `InventJournalPostingService`, `InventPosting` | Low — dependent on the model above |
| Physical dimension buckets | `Received`, `Deducted` and `Picked` are maintained by receipt, sales and picking flows. `Registered` and `Arrived` have no confirmed writer. | Arrival registration, when used, maintains its physical state. | Arrival/registration process, if required by the deployment. | `InventSum`, inbound arrival flow | Low — conditional |
| Inventory status dimension | `InventDim.InventStatusId` is stored, `WHSInventStatus` setup exists, and blocked status excludes stock from available on-hand and blocks sales demand and journal issues. | Status is a full inventory dimension with its own setup, conversion and transfer behavior. | Status change/transfer processes; applicability depends on which dimensions are enabled. | `WHSInventStatus`, `InventoryDimensionPolicy` | Medium |
| SQL Server verification | Inventory behavior is covered by SQLite in-memory tests. The transfer-origin relation migrations are generated but not applied to SQL Server. | — | Transaction atomicity under SQL Server, duplicate/concurrent posting, number-sequence contention, migration application, and handling transfer orders created before the new lifecycle are unverified. | Whole module, `src/Infrastructure/Migrations` | **Critical** — nothing here should be called operationally ready until this is done |
| Quantity splitting for marking | Marking links whole transactions; a side already split across movements is rejected with a named error. | Part of a quantity is marked by **splitting** the inventory transaction. | Transaction splitting on marking. Deliberately not approximated with an invented marked-quantity column, because D365 has no such column. | `InventoryMarkingService.LoadSideAsync` | Low |

## Deliberate restrictions (verified in code)

These are not gaps to be fixed silently; they are enforced scope boundaries. Each raises the exact
message quoted.

| Restriction | Message |
| --- | --- |
| Settled models | `The basic inventory release settles {FIFO, LIFO, WeightedAvg} only.` |
| Date-based models at posting | `Inventory model '{model}' has no settlement engine, so a journal cannot be posted for it. Use FIFO, LIFO or Weighted average.` |
| Batch / serial tracking | `Batch and serial tracking are outside the basic inventory scope.` |
| Location / advanced storage dimensions | `Location and advanced storage dimensions are outside the basic inventory scope.` |
| Counting journal dimensions | `The basic counting journal supports site and warehouse dimensions only.` |
| Counting candidates on non-basic dimensions | `Item '{itemId}' holds stock on a batch, serial, status or location dimension, which the basic counting journal does not count.` |
| Negative inventory | `Negative physical and financial inventory are disabled in the basic release.` |
| Financial posting | `Financial inventory posting is required in the basic release.` |
| Include physical value in cost | `Include physical value in cost is disabled in the basic release.` |
| Ordered reservation | `Reservation against ordered supply is disabled in the basic inventory scope.` |
| Automatic journal reservation | `Automatic journal reservation is outside the basic inventory scope.` |
| Product subtypes | `Only distinct Item products are supported in the basic inventory scope.` |
| Unit conversions | `Only positive linear unit conversions are supported for basic inventory.` |
| Backdated close | `A later or equal inventory close already exists; backdated close is not supported in the basic scope.` |
| Reporting currency at close | `Inventory close with a different reporting currency requires exchange-rate processing, which is not enabled in the basic inventory scope.` |
| Mixed journal dates | `All lines must use the same transaction date for basic journal posting.` |
| Stocked products | `The basic inventory model group must contain stocked products.` |

## Flagged uncertainties

- **Existing transfer orders at upgrade.** Orders created by the earlier implementation have no
  ordered source/destination transactions or line-origin relations. A data backfill or an explicit
  compatibility path is needed before the new shipment/receipt code can process those orders;
  existing production data was not available to verify a safe backfill.
- **Inventory status applicability.** Whether status setup and lookup should be exposed at all
  depends on which inventory dimensions are enabled in the storage dimension group. Not verified in
  a live D365 environment; only expose statuses when the dimension is enabled.
- **Counting line generation cap.** `MaxGeneratedCountingLines = 1000` is a local safeguard, not a
  verified D365 limit. D365's own limit, if any, is unknown.
- **`InventTrans.CostAmountPhysical` sign convention on returns.** The return flows round to two
  decimals away from zero; whether D365 rounds at the transaction or at the settlement is unverified.

## Corrections of record

- An earlier revision of `README.md` recorded a missing `InventMark` table. **There is no such table
  in D365.** Marking lives on `InventTrans.MarkingRefInventTransOrigin`, which already existed here.
  The real gap was that nothing ever wrote the field.
- An earlier revision recorded `InventSum.PhysicalValue` as never written. It is now maintained end
  to end by the accounts payable and receivable posting flows.
- The model-group service previously rejected every model except FIFO, which made the weighted
  average and LIFO settlement paths unreachable. Setup and the close engine now share one list.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-02 | A transfer posting request ID is idempotent only for its original shipment or receipt operation. Reusing it for the opposite operation now returns a named error instead of silently skipping the movement. `ORDER` is reserved for initial order transactions. |
| 2026-11-02 | Transfer-order creation now writes ordered source and destination inventory transactions and ordered on-hand buckets. Shipment updates or splits those line-linked transactions, reduces source on-order quantity, and creates the transit pair. Added standard `InventTransOriginTransferShip` relation. |
| 2026-11-02 | Added the standard `InventTransOriginTransferTransitTo`, `InventTransOriginTransferTransitFrom`, and `InventTransOriginTransferReceive` relations. Transfer receipt and transit costing now select the requested line's origins, so duplicate item lines do not consume each other's pending transactions or cost. |
| 2026-11-02 | Transfer shipment creates a reserved-physical transit issue and ordered destination receipt in addition to the financially posted source/transit pair. Receipt updates those pending legs to Sold/Purchased, splitting a pending pair for partial receipts, and maintains transit reservations and destination ordered quantity. |
| 2026-11-01 | Transfer-order receipt now carries the order's remaining posted transit value to the destination instead of recalculating from the current site average. Later purchases and other orders in transit cannot reprice this order's receipt. |
| 2026-11-01 | Inventory close now verifies balanced same-site transfer-order issue and receipt legs by order, voucher and item, then excludes those internal movements from cost settlement. Incomplete or unbalanced movements are rejected. |
| 2026-11-01 | Transfer orders now reject cost models that the basic inventory close engine cannot settle instead of moving their stock at an unrelated running-average cost. |
| 2026-11-01 | Transfer-order create and posting now require a stocked item model group; released nonstocked items can no longer create physical inventory movements. |
| 2026-11-01 | Transfer-order creation now checks the item's inventory lifecycle block; shipment and receipt recheck it and protect both inventory dimensions against a frozen count and a closed inventory date before moving stock. |
| 2026-11-01 | Repeating the final transfer-order receipt with the same request ID now returns the posted order without moving stock again; the duplicate check runs before the fully-received status guard. |
| 2026-11-01 | Summary journal vouchers are allocated only when a line creates a movement. A confirmed count with no variance now posts without consuming a voucher or creating inventory and ledger transactions. |
| 2026-11-01 | Reposting an already posted summary journal now returns its single voucher once while creating no additional inventory or ledger transactions. |
| 2026-11-01 | Released-product configuration and unit assignment now refuse an item without an existing shared `EcoResProduct` before writing conversions; this prevents a legacy `Product = 0` item from creating a global standard conversion. |
| 2026-11-01 | Released-product unit assignment now validates commercial values, tax groups, and default order settings before changing tracked item groups or units; a rejected setup request no longer leaves pending mutations in the current unit of work. |
| 2026-11-01 | Confirming a counting journal now records the counted-minus-frozen-on-hand difference in both inventory `Qty` and selected-unit `UnitQty`; posting recomputes and verifies the difference against the frozen balance. |
| 2026-11-01 | Sales and purchase document quantities now honor the selected unit-conversion rounding rule (nearest, up, or down) when deriving inventory quantity. Removed the item-record-ID fallback for product-specific conversions; an item without a shared product can use only a standard conversion. |
| 2026-11-01 | Physical journal line validation now requires a stocked item model group for every item; previously a non-service item without an assignment could be saved and would fail only at posting. |
| 2026-11-01 | Inventory journal lines now store both selected-unit `UnitQty` and inventory-unit `Qty`; posting uses the inventory quantity without converting it again. Journal conversion applies product-specific rules before standard rules and honors the configured rounding mode and destination-unit precision. |
| 2026-11-01 | Corrected the stale Picked-bucket claim after checking `InventoryReservationService.Picking`. Released-item validation now accepts all three supported stocked cost models. Purchase-order line creation now checks the configured product lifecycle purchase block. |
| 2026-11-01 | Added the verified D365 `InventAccessLevel` and `InventUserGroupInfo` period fields and an EF migration; inventory journal and close posting now check Inventory module access independently. Field names verified against Microsoft Common Data Model. |
| 2026-11-01 | Removed a second FIFO-only restriction in released-product configuration; its model-group check now accepts FIFO, LIFO, and weighted average, matching setup and close. |
| 2026-11-01 | Inventory setup readiness now recognizes configured units, dimension groups, item groups, supported stocked model groups, and posting profiles by their actual setup instead of requiring sample IDs (`EA`, `SITE-WH`, `NONE`, `TRADE`, `FIFO-BASIC`). |
| 2026-11-01 | Corrected provisional journal, transfer-order, sales packing-slip and sales-invoice costing to use the financially tracked site pool for FIFO, LIFO and weighted average. Physical value is excluded because the basic model group disables include-physical-value-in-cost; the configured item cost is the fallback when the pool has no positive financial value and quantity. |
| 2026-11-01 | Positive movement and adjustment journal lines now default a blank cost price from the released item's inventory cost price; posting still refuses a receipt when neither line nor item has a positive cost. |
| 2026-11-01 | File created. Recorded the value report's physical/financial split, retired standard-cost variance to a dependent gap, and added the ledger-period-access gap found while adapting to the `userId` posting change. |
| 2026-11-01 | Fixed: the journal posting engine rejected weighted average although model-group setup accepts it, so no journal could be posted for a weighted-average item. Now only the date-based variants are rejected. |

## Sources

- [Inventory journals](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals)
- [Journal unit quantity](https://learn.microsoft.com/en-us/troubleshoot/dynamics-365/supply-chain/inventory/journal-unit-quantity-incorrect)
- [Count inventory in a warehouse](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/tasks/count-inventory-warehouse)
- [Unit conversions and rounding](https://learn.microsoft.com/en-us/dynamics365/supply-chain/pim/tasks/manage-unit-measure)
- [Physical and financial updates](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates)
- [Inventory close](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-close)
- [Running average cost](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/running-average-cost-price)
- [InventTrans in Common Data Model](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transaction/inventtrans)
