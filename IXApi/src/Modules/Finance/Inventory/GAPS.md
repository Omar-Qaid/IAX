# Inventory — gap analysis against basic D365 F&O

**Last updated:** 2026-11-01
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
| Transfer orders (`InventTransferTable`/`Line`) | Implemented |
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
| Ledger period access for inventory | Inventory journal and close posting pass `LedgerPostingModule.Ledger`, so inventory postings are governed by the **Ledger** module's period access. | The ledger calendar has its own Inventory module row, controlled independently of Ledger. | No `InventAccessLevel`/`InventUserGroupInfo` column exists on `LedgerPeriodModuleAccessControl`, so the dimension cannot be represented. Adding it is a schema change and was not invented here. | `LedgerPeriodModuleAccessControl`, `SubledgerPostingSupport.LedgerPostingModule`, `InventCloseService`, `InventJournalPostingService` | Medium |
| Physical dimension buckets | `Received` and `Deducted` are maintained by the receipt/invoice and packing-slip/invoice flows. `Registered`, `Arrived` and `Picked` are never written. | Arrival registration and picking maintain these buckets and they feed available quantity. | Arrival/registration and picking processes. No such process exists in this repository, so the buckets have no writer. | `InventSum`, arrival and picking flows (absent) | Low — basic WHS-adjacent; `AvailPhysical` already subtracts `Picked`, so the formula is ready |
| Inventory status dimension | `InventDim.InventStatusId` is stored, `WHSInventStatus` setup exists, and blocked status excludes stock from available on-hand and blocks sales demand and journal issues. | Status is a full inventory dimension with its own setup, conversion and transfer behavior. | Status change/transfer processes; applicability depends on which dimensions are enabled. | `WHSInventStatus`, `InventoryDimensionPolicy` | Medium |
| SQL Server verification | All inventory behavior is covered by SQLite in-memory tests. | — | Transaction atomicity under SQL Server, duplicate/concurrent posting, number-sequence contention, and migration application are unverified. Both approved migrations are code only and have not been applied. | Whole module, `src/Infrastructure/Migrations` | **Critical** — nothing here should be called operationally ready until this is done |
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
| 2026-11-01 | File created. Recorded the value report's physical/financial split, retired standard-cost variance to a dependent gap, and added the ledger-period-access gap found while adapting to the `userId` posting change. |
| 2026-11-01 | Fixed: the journal posting engine rejected weighted average although model-group setup accepts it, so no journal could be posted for a weighted-average item. Now only the date-based variants are rejected. |

## Sources

- [Inventory journals](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals)
- [Physical and financial updates](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates)
- [Inventory close](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-close)
- [Running average cost](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/running-average-cost-price)
- [InventTrans in Common Data Model](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transaction/inventtrans)
