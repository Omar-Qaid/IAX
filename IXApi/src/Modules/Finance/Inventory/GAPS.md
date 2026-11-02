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
| Posted transfer journal (`InventTransferJour`), sequence-allocated transfer id and voucher | Implemented |
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
| Legacy moving-average seed | New setup seeds supported FIFO and weighted-average groups. An existing `MA` group retains its moving-average model and is not usable by basic inventory posting. | Existing costing models must not be silently changed. | A deployment that used the legacy group needs a separately verified costing-model transition before posting. | `InventorySeeder.SeedSetupGroupsAsync` | Medium — explicitly unsupported; existing valuation configuration is preserved |
| Inventory models | FIFO, LIFO and weighted average are accepted at model-group setup and settled by inventory close. Standard cost, moving average, LIFO-date and weighted-average-date are rejected with a named error. | All of these are basic inventory models, each with its own valuation and close behavior. | The four rejected models, and with them standard-cost revaluation and moving-average maintenance. | `InventModelGroupService.SettledModels`, `InventCloseService.SupportedModels`, `InventJournalPostingService.ResolveUnitCostAsync` | Medium — rejected explicitly, not approximated, so nothing is silently wrong |
| Standard-cost variance posting | `InventJournalPostingService` rejects a receipt priced differently from the active standard cost. Unreachable in practice, because standard cost is rejected at setup. | Purchase price variance posts to a variance account per the posting profile. | Variance calculation and posting. Blocked behind the standard-cost model itself. | `InventJournalPostingService`, `InventPosting` | Low — dependent on the model above |
| Physical dimension buckets | `Received`, `Deducted` and `Picked` are maintained by receipt, sales and picking flows. `Registered` and `Arrived` have no confirmed writer. | Arrival registration, when used, maintains its physical state. | Arrival/registration process, if required by the deployment. | `InventSum`, inbound arrival flow | Low — conditional |
| SQL Server verification | `InventSum` inherits the shared SQL Server rowversion concurrency token. A unique company/item/dimension index prevents duplicate on-hand summaries once its migration is applied. The earlier migration chain was tested on a fresh SQL Server database; this new index and concurrent runtime behavior have not been verified there. | — | Test atomicity, conflicting updates and duplicate creation on SQL Server, number-sequence contention, and transfer orders created before the new lifecycle. Duplicate existing summaries must be resolved before applying the new unique index. | Whole module, `src/Infrastructure/Migrations` | **High** — concurrent runtime behavior still unproven |
| Later recalculation when closing | Closing automatically reverses later overlapping recalculations within the close transaction. An item-scoped recalculation may be reversed past later recalculations for other items; overlapping, company-wide and close runs still enforce ordering. An item close refuses to reverse a company-wide recalculation. | D365 automatically cancels recalculations dated after the closing date. [Microsoft inventory costing FAQ](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-costing-faq). | Scoped reversal of a company-wide recalculation and cases blocked by the remaining ordering rules require further verification. | `InventCloseService`, cancellation and ledger correction | Medium — unsupported cases are refused |
| Original accounts for cost adjustments | Inventory journal, newly invoiced sales issues and sales-return receipts use the original financial `InventTransPosting` record. Purchase returns requiring a nonzero cost adjustment are refused; older sales invoices and credits without a unique financial posting record are also refused. | Close and recalculation adjust the accounts used by the original transaction, even if profiles changed. [Microsoft inventory close](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-close). | Verify and implement original-account and offset treatment for purchase-return cost adjustments; establish a safe historical recovery path for sales invoices and credits without posting records. | Purchase return posting, `InventTransPosting`, `InventCloseService` | High — affected purchase returns cannot be closed until supported |
| Sales returns against partial invoices | Return lines retain the inventory origin but do not persist the selected invoice line. Creation locks the original origin while checking entitlement and caps each request against the selected invoice line; it cannot attribute earlier returns to individual invoices. Physical return creation and close refuse differing invoice costs for that origin rather than averaging them. | A return tied to an invoice uses its original sale cost. [Microsoft return cost price](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/return-cost-price-and-return-lot-id). | Verify and persist the standard selected-invoice relationship; use it for cumulative invoice-specific entitlement and valuation, including historical returns. | `CustReturnService`, sales return line, inventory close | High — affected partial-invoice returns are refused or can exceed a selected invoice across separate requests |
| Marking a side split across movements | `LoadSideAsync` rejects a side whose `InventTransOrigin` owns more than one `InventTrans` row, with a named error. | Marking is per transaction; D365 splits a transaction to mark part of a quantity. | Only the ability to mark **one chosen movement** of an already-split side. Selecting it needs a caller-supplied transaction identifier, so it is an API change, not a costing change. | `InventoryMarkingService.LoadSideAsync` | Low |

## Deliberate restrictions (verified in code)

These are not gaps to be fixed silently; they are enforced scope boundaries. Each raises the exact
message quoted.

| Restriction | Message |
| --- | --- |
| Legacy recalculation settlement | `An earlier inventory recalculation contains settled quantities or costs. Reverse affected cost runs in reverse order before running inventory recalculation or close.` |
| Item close after company recalculation | `An item close cannot automatically reverse a company-wide recalculation. Reverse later cost runs in reverse order before closing this item.` |
| Journal correction without original posting | `The original financial inventory posting for transaction {issue.RecId} could not be uniquely identified.` |
| Purchase-return cost adjustment | `Purchase return cost adjustment requires the original inventory posting accounts; this accounting path is not implemented.` |
| Sales return invoice costs | `The original sales invoices for item '{line.ItemId}' have different inventory costs; the selected invoice cannot be identified for return costing.` |
| Partially marked weighted-average issue | `A partially marked weighted-average issue cannot be costed until its marked quantity is known.` |
| Cost adjustment without on-hand summary | `Inventory summary is missing for item '{itemId}' and inventory dimension '{dimId}'; the cost adjustment cannot be posted.` |
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

## Out of scope (recorded so they are not re-raised as gaps)

| Area | Why it is out of scope |
| --- | --- |
| Inventory status change and transfer processes | Inventory status is a **Warehouse management** dimension in D365 — hence the `WHSInventStatus` table name — and advanced WHS is excluded. The basic storage dimension group here carries site and warehouse only and has no status flag. What exists (status stored on `InventDim`, blocked status excluded from available on-hand and refused for sales demand and journal issues) is kept and is sufficient; status conversion and transfer processes are not basic inventory. |
| Financial dimension derivation | Derived dimensions, derivation rules and automatic population of dimensions through derivation are excluded by request. Basic financial dimensions and account structures remain in scope. `InventSite.DefaultDimension` is therefore stored but intentionally not consumed, and is deliberately left unchanged. |
| Inventory journal name defaulting from `InventParameters` | The four journal-name parameters are validated for existence and for an allocatable voucher sequence. Applying them as a *default* is a D365 form behavior, not a posting or validation rule, and implementing it at this layer would require adding a journal-type input to the create API. Not a service-layer gap. |

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
| 2026-11-02 | Item-scoped recalculations can be reversed past later recalculations for other items; close and overlapping or company-wide runs retain their ordering protection. |
| 2026-11-02 | Serialized customer return entitlement checks by original inventory origin and capped a single request against each selected invoice line. Cumulative invoice-specific entitlement and invoice-specific costing remain open. |
| 2026-11-02 | Released-item readiness now checks the account types each cycle actually resolves when it posts: the sales set gained `SalesConsump`, and each physical accrual offset is required when the item's own model group enables the matching physical ledger posting. Previously an item could report purchase- or sales-ready and then fail at the first product receipt or packing slip. Complements the same check at company level. |
| 2026-11-02 | Chart of accounts seeds `21010000` *Accrued Purchases - Product Receipt* and `21020000` *Accrued Cost of Goods Sold - Packing Slip*, and the inventory posting seed maps `PurchPackingSlipOffsetAccount` and `SalesPackingSlipOffsetAccount` to them. Both offsets are resolved by `PurchaseReceiptAccrualPosting` and `PackingSlipPhysicalPosting` but were previously absent from seed data, so enabling either physical-posting flag broke posting on a freshly seeded company. |
| 2026-11-02 | Enforced one `InventSum` row per company, item and inventory dimension using a unique index; existing SQL Server rowversion already detects conflicting updates. SQL Server concurrency and the new migration remain to be verified. |
| 2026-11-02 | Fresh inventory setup seeds supported FIFO and weighted-average stocked groups without mandatory receiving. The legacy FIFO seed is repaired only when its basic identifying fields match; existing moving-average groups retain their costing model. Company readiness no longer accepts a model group that requires receiving. |
| 2026-11-02 | Company inventory readiness checks the sales consumption account and requires each physical accrual offset when a stocked model group enables the corresponding physical ledger posting. |
| 2026-11-02 | Sales return creation counts unsaved earlier lines in the same request when checking the original sales line's invoiced quantity, and rolls back the request if their combined quantity exceeds it. Invoice-specific entitlement remains an open gap. |
| 2026-11-02 | Physical sales returns and inventory close refuse ambiguous differing costs across partial invoices until the selected invoice relationship is supported, instead of averaging unrelated invoice costs. |
| 2026-11-02 | Weighted-average closing uses the selected receipt cost for a fully marked issue and for a vendor return linked to its original receipt, instead of applying the unmarked site average. Partially marked weighted-average issues are refused until their marked quantity can be identified. |
| 2026-11-02 | Multi-receipt cost settlement allocates persisted four-decimal amounts with the final row carrying the remainder, so recorded settlement adjustments sum to the posted amount and reversals restore it exactly. |
| 2026-11-02 | Cost adjustments and their reversals refuse a missing item/dimension on-hand summary and roll back, preventing the ledger and inventory transaction from changing without the corresponding inventory value. |
| 2026-11-02 | Reject nonzero purchase-return cost adjustments until their original-account and offset accounting can be verified and implemented. Runs roll back instead of resolving the current posting profile. |
| 2026-11-02 | Sales return credits record their financial inventory account pair; later return receipt adjustments and reversals use that original record. Missing or ambiguous historical records are refused. |
| 2026-11-02 | Sales invoices record the financial inventory account pair for each invoice voucher; later issue cost adjustments and reversals use that original record. Missing or ambiguous historical records are refused. |
| 2026-11-02 | Inventory journal close, recalculation, and reversal corrections use accounts and posting types from the original financial posting record; a missing or ambiguous record is refused. Other transaction sources remain open. |
| 2026-11-02 | Closing automatically reverses eligible later recalculations within the same transaction. Item closes refuse company-wide recalculations rather than reversing other items; global reversal ordering remains enforced. |
| 2026-11-02 | Cost runs reject active legacy recalculations that recorded settlement quantities or costs for the affected item. Reverse those runs through the existing cancellation operation before recalculating or closing; no historical data is changed automatically. |
| 2026-11-02 | Cost-run numbering now includes reversal records when allocating the next run number. A close after reversing an earlier close no longer reuses the reversal's run number; cancellation records remain excluded from active-close checks. |
| 2026-11-02 | Verified the whole migration chain against a fresh SQL Server database (`IXApi_MigrationCheck` on the local instance), including the four previously unapplied migrations, and confirmed `InventTransferJour`'s three unique indexes and `VendParameters`' unique keys in `sys.indexes`. Downgraded the SQL Server row from Critical to High: migrations are proven, concurrent runtime behavior is not. |
| 2026-11-02 | Established that quantity-partial marking already produces D365's costing outcome without transaction splitting: the close engine consumes the marked receipt layer by quantity and lets the remainder follow the model order, writing one `InventSettlement` row per layer. Transaction splitting was therefore **not** implemented — it would change the stored row shape without changing any quantity, cost or ledger amount. The open row was narrowed to marking one chosen movement of an already-split side, which is an API change. |
| 2026-11-02 | Added an *Out of scope* section and moved three rows into it: inventory status change/transfer (a Warehouse management dimension, excluded with advanced WHS), financial dimension derivation (excluded by request), and inventory journal name defaulting (a form default, not a service rule). Recorded so they are not re-raised as gaps. |
| 2026-11-02 | Added the standard `InventTransferJour` posted transfer journal (one row per posted shipment or receipt). The posting voucher is now allocated from the new `InventTransferJour` number sequence instead of being composed as `{TransferId}:{requestId}`, so the caller no longer chooses the voucher and it stays unique and joinable to the ledger. The duplicate-request guard moved onto that journal. `InventTransferJour.PostingRequestId` is a **deliberate local extension** — D365 has no such field and prevents repeated posting through the line's remaining quantity alone; it is kept so the existing exactly-once `requestId` API contract survives the move. |
| 2026-11-02 | Transfer-order creation now allocates a blank `TransferId` from the new `InventTransferTable` number sequence; an explicitly supplied identifier is still accepted as the manual-sequence case. |
| 2026-11-02 | Inventory parameters are no longer seeded with `ReserveOnOrdered = Yes`, which `InventParametersService` rejects and which made a freshly seeded company report `MISSING_INVENTORY_PARAMETERS` and never become ready for item setup. The repeatable seeder also converges an existing company's flag. |
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
