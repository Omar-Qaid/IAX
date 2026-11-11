# Inventory — gap analysis against basic D365 F&O

**Last updated:** 2026-11-11
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
| Reference cycle (`CycleErpExamples/Inventory/Logic_Readonly.md`) executed end to end | Implemented and **tested** |
| Batch tracking across purchase receipt and sales issue (`MD-04`) | Implemented and **tested** (SQLite); authorized explicitly |
| Serial tracking across purchase receipt and on-hand (`MD-04`) | Implemented and **tested** (SQLite), including the one-unit-per-line rule; no serial sales-issue scenario is verified |
| SQL Server behavior | **Unverified for this module** — SQLite tests only. Only the AP cycle has been run on SQL Server (2026-11-04); `BatchDatabaseLock` mutual exclusion is verified there, but no AR/GL/Inventory flow is. |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Tracking dimensions on a sales return | `CustReturnService` copies the sold movement's dimension onto the return line (`InventDimId = sold.InventDimId`), so a batch or serial returns to the dimension it was issued from. Verified by inspection only. | A returned batch or serial re-enters stock under its own dimension. | A test that returns a batch-tracked item and asserts the return movement's dimension and the per-batch on-hand. MD-04 verifies receipt, reservation and issue; the return half is not exercised. | `CustReturnService`, `InventoryDimensionPolicy` | Low — the code preserves the dimension by construction; only the proof is missing |
| Fiscal-period refusal on an inventory posting | `SubledgerPostingSupport.LoadLedgerContextAsync` refuses a posting with `The fiscal period for {date:d} is not open.`, `No operating fiscal period contains {date:d}.`, `The {module} module is closed for the fiscal period containing {date:d}.` and a user-group refusal. Only the "is not open" message is asserted anywhere, and only through AP and AR tests. | Posting into a closed period, or a period whose module is closed, is refused. | An inventory-path test for each refusal, and for the module and user-group guards which no test reaches. `LedgerPeriodModuleAccessControl` is also absent from the test `CycleContext`, so the module guard is skipped there entirely. | `SubledgerPostingSupport`, `LedgerPeriodModuleAccessControl` | Medium — the guards exist and are unproven for this module (`CFG-04`) |
| Same-period sales return close | FIFO close can settle an earlier sales issue before costing its return when receipts before the first return cover all preceding issues and the pool has no marking or purchase returns. Later non-return receipts are allowed. Other cases still require the original issue to have been settled already. | Return cost follows the original sales issue's cost. | Support and verify same-period return dependencies for other costing models, mixed pools and cases where earlier receipt quantities are insufficient. | `InventCloseService.SettlePoolAsync` | Medium — unsupported cases are refused |
| Count quantity storage precision | Count confirmation validates against the current inventory unit and rejects quantities beyond four decimals because inventory quantities use `decimal(18,4)`. Units may be configured with up to six decimals. | Quantities should honor the configured unit precision. | Verify and expand precision across the complete inventory quantity flow before accepting five or six decimals. | Counting journals, inventory transactions and on-hand quantities | Medium — affected values are refused rather than rounded silently |
| Weighted-average settlement representation | Close carries the remaining value into later averages using non-cancelled receipt settlement costs. If the receipt-layer value differs after a prior average close, a marked issue or purchase return against those original layers is refused. | Remaining inventory is carried into the next period at the preceding weighted-average value. D365 normally uses a weighted-average closing transfer for summarized settlements. [Microsoft weighted average](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/weighted-average-physical-value-marking). | Implement and verify the standard closing-transfer representation, including linked receipt cases and cancellation. | `InventCloseService.SettlePoolAsync` | Medium — ambiguous linked-receipt cases are refused |
| Legacy moving-average seed | New setup seeds supported FIFO and weighted-average groups. An existing `MA` group retains its moving-average model and is not usable by basic inventory posting. | Existing costing models must not be silently changed. | A deployment that used the legacy group needs a separately verified costing-model transition before posting. | `InventorySeeder.SeedSetupGroupsAsync` | Medium — explicitly unsupported; existing valuation configuration is preserved |
| Inventory models | FIFO, LIFO and weighted average are accepted at model-group setup and settled by inventory close. Standard cost, moving average, LIFO-date and weighted-average-date are rejected with a named error. | All of these are basic inventory models, each with its own valuation and close behavior. | The four rejected models, and with them standard-cost revaluation and moving-average maintenance. | `InventModelGroupService.SettledModels`, `InventCloseService.SupportedModels`, `InventJournalPostingService.ResolveUnitCostAsync` | Medium — rejected explicitly, not approximated, so nothing is silently wrong |
| Standard-cost variance posting | `InventJournalPostingService` rejects a receipt priced differently from the active standard cost. Unreachable in practice, because standard cost is rejected at setup. | Purchase price variance posts to a variance account per the posting profile. | Variance calculation and posting. Blocked behind the standard-cost model itself. | `InventJournalPostingService`, `InventPosting` | Low — dependent on the model above |
| SQL Server verification | `InventSum` inherits the shared SQL Server rowversion concurrency token. A unique company/item/dimension index prevents duplicate on-hand summaries once its migration is applied. The earlier migration chain was tested on a fresh SQL Server database; this new index and concurrent runtime behavior have not been verified there. | — | Test atomicity, conflicting updates and duplicate creation on SQL Server, number-sequence contention, and transfer orders created before the new lifecycle. Duplicate existing summaries must be resolved before applying the new unique index. | Whole module, `src/Infrastructure/Migrations` | **High** — concurrent runtime behavior still unproven |
| Original accounts for cost adjustments | Inventory journals, sales issues, sales-return receipts and purchase returns record their financial `InventTransPosting` pair, and close reuses it. A source whose posting always records the pair is still refused when no unique record exists, which affects sales invoices and credits posted before records were written. | Close and recalculation adjust the accounts used by the original transaction, even if profiles changed. [Microsoft inventory close](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-close). | A safe historical recovery path for pre-existing sales invoices and credits with no posting record. | `InventTransPosting`, `InventCloseService` | Medium — only historical documents without a record are affected |
| Sales returns against partial invoices | `SalesLine.OriginalInvoiceTransRecId` records the invoice line a return was created from. Creation, credit costing and close all use that invoice; entitlement is counted per invoice line, falling back to the conservative origin-wide quantity only while an unattributed legacy return exists. A line created before the field existed is still refused when its origin's invoices carried different costs. | A return tied to an invoice uses its original sale cost. [Microsoft return cost price](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/return-cost-price-and-return-lot-id). | A migration or recovery path for return lines created before the link existed. | `CustReturnService`, `SalesLine`, `InventCloseService` | Low — only pre-existing return lines are affected |
| Marked layers competing with other issues | FIFO/LIFO close reserves the open quantity of a wholly marked issue, permits other issues to consume excess receipt quantity, and refuses ambiguous partial or split marking. Weighted-average close excludes marked quantities from the unmarked average and carries remaining value forward; it refuses a linked receipt whose value is ambiguous after a previous average close. Pools mixing marked issues and purchase returns are also refused. | Marked receipt and issue quantities are settled together ahead of the inventory model. [Microsoft inventory costing FAQ](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-costing-faq). | Identify partial marked quantities and verify interactions with purchase returns and the standard weighted-average closing transfer. | `InventCloseService.SettlePoolAsync` | Medium — supported cases are protected; ambiguous cases are refused |
| Marking a side split across movements | Marking and unmarking can select a movement by `InventTrans.RecId`. Unmarking refuses missing or ambiguous counterparts; a second pair between the same origins is refused because origin references alone do not distinguish the pairs. Close still refuses a marked receipt split across multiple open movements. | Marking is per transaction; D365 splits a transaction to mark part of a quantity and settles marked movements together. | Support split receipt settlement and multiple distinguishable marking pairs without guessing counterpart movements. Verify marking and close concurrency on SQL Server. | `InventoryMarkingService`, `InventCloseService.SettlePoolAsync` | Medium — ambiguous cases are refused |

## Audited and found sound (2026-11-10)

Recorded so these are not re-investigated. Each was checked and raised no defect.

- **Seeder dependency graph**, `SeedingERPV2/Modules/Inventory`: 24 seeders, every `RequireAsync` type ordered
  by a declared dependency. Five apparent gaps were false positives — four satisfied transitively through the
  matching `FldSetup` seeder (`SeedDependencyResolver` is a depth-first topological sort with cycle detection),
  and `InventPostingSeeder` does declare `ERPV2.DimensionAttributeValueCombinationSeeder`, whose rows are
  written through a service rather than `EnsureAsync`. No dependency names a seeder that does not exist.
- **On-hand bucket formulas**: all 11 `AvailPhysical` and all 16 `AvailOrdered` assignments across Finance use
  one formula each (`PhysicalInvent - ReservPhysical - Picked`, and `AvailPhysical + Ordered - OnOrder -
  ReservOrdered`). No divergence. `InventTransferOrderService` reuses `AvailPhysical` without recomputing it when
  creating a transfer order, which is correct there: ordering moves only `OnOrder` and `Ordered`, neither of which
  feeds `AvailPhysical`. The formulas are duplicated rather than shared, so a future change must be applied to all
  of them; that is a reuse risk, not a current defect.
- **Cost rounding in closing and settlement**: `InventCloseService` keeps layer unit cost and the pool average at
  full decimal precision and rounds only at the adjustment boundary with
  `decimal.Round(..., 4, MidpointRounding.AwayFromZero)`, which avoids compounding rounding error. The unrounded
  `UnitCost` in `InventTransMapping` is a read-model projection, not a posted value.
- **Company scoping of module queries**: `ApplicationDbContext` applies a global `DataAreaId` query filter to
  every `IMultiCompany` entity except six shared ones, so an Inventory query without an explicit company
  predicate — such as the batch and serial existence checks in `InventoryDimensionPolicy` — is already scoped.
  The test `CycleContext` does not apply that filter, which is why tests set `DataAreaId` explicitly.

## Deliberate restrictions (verified in code)

These are not gaps to be fixed silently; they are enforced scope boundaries. Each raises the exact
message quoted.

| Restriction | Message |
| --- | --- |
| Legacy recalculation settlement | `An earlier inventory recalculation contains settled quantities or costs. Reverse affected cost runs in reverse order before running inventory recalculation or close.` |
| Item close after company recalculation | `An item close cannot automatically reverse a company-wide recalculation. Reverse later cost runs in reverse order before closing this item.` |
| Journal correction without original posting | `The original financial inventory posting for transaction {issue.RecId} could not be uniquely identified.` |
| Sales return invoice costs, line predating the invoice link | `The original sales invoices for item '{line.ItemId}' have different inventory costs, and this return line predates the selected-invoice link; the invoice cannot be identified for return costing.` |
| Partially marked weighted-average issue | `A partially marked weighted-average issue cannot be costed until its marked quantity is known.` |
| Cost adjustment without on-hand summary | `Inventory summary is missing for item '{itemId}' and inventory dimension '{dimId}'; the cost adjustment cannot be posted.` |
| Settled models | `The basic inventory release settles {FIFO, LIFO, WeightedAvg} only.` |
| Dimensions outside the basic scope | `Only site, warehouse, batch, and serial dimensions are supported in the basic inventory scope.` |
| Batch required by the tracking group | `A batch number is required for item '{itemId}'.` |
| Serial required by the tracking group | `A serial number is required for item '{itemId}'.` |
| Undefined batch | `Batch '{batchId}' is not defined for item '{itemId}'.` |
| Undefined serial | `Serial number '{serialId}' is not defined for item '{itemId}'.` |
| Serial line quantity | `Each line for serial-tracked item '{itemId}' must identify one serial-numbered unit.` |
| Batch or serial on an untracked item | `Item '{item.ItemId}' has no tracking dimension group, so a batch or serial number cannot be recorded.` |
| Date-based models at posting | `Inventory model '{model}' has no settlement engine, so a journal cannot be posted for it. Use FIFO, LIFO or Weighted average.` |
| Batch / serial tracking | `Batch and serial tracking are outside the basic inventory scope.` |
| Location / advanced storage dimensions | `Location and advanced storage dimensions are outside the basic inventory scope.` |
| Counting journal dimensions | `The basic counting journal supports site and warehouse dimensions only.` |
| Counting candidates on non-basic dimensions | `Item '{itemId}' holds stock on a batch, serial, status or location dimension, which the basic counting journal does not count.` |
| Negative inventory | `Negative physical and financial inventory are disabled in the basic release.` |
| Financial posting | `Financial inventory posting is required in the basic release.` |
| Include physical value for models other than periodic weighted average | `Include physical value is supported for periodic weighted average only; FIFO/LIFO physical close adjustments are not implemented.` |
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
| Arrival registration and the `Registered` / `Arrived` buckets | These buckets are maintained by the item arrival and production input journals, which update inventory **physically only** and set the receipt status to `Registered` ([inventory posting](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/inventory-posting)). D365 requires that step only when a model group sets registration or receiving requirements, and `InventModelGroupService` deliberately refuses `MandatoryRegister`, `MandatoryReceive`, `MandatoryPick` and `MandatoryDeduct`, so no basic flow can demand it. The two buckets therefore have no writer by design, not by omission. Enabling them means adding the arrival journal, which is a new transaction type outside the basic receipt/issue cycle. |
| Inventory status change and transfer processes | Inventory status is a **Warehouse management** dimension in D365 — hence the `WHSInventStatus` table name — and advanced WHS is excluded. The basic storage dimension group here carries site and warehouse only and has no status flag. What exists (status stored on `InventDim`, blocked status excluded from available on-hand and refused for sales demand and journal issues) is kept and is sufficient; status conversion and transfer processes are not basic inventory. |
| Financial dimension derivation | Derived dimensions, derivation rules and automatic population of dimensions through derivation are excluded by request. Basic financial dimensions and account structures remain in scope. `InventSite.DefaultDimension` is therefore stored but intentionally not consumed, and is deliberately left unchanged. |
| Inventory journal name defaulting from `InventParameters` | The four journal-name parameters are validated for existence and for an allocatable voucher sequence. Applying them as a *default* is a D365 form behavior, not a posting or validation rule, and implementing it at this layer would require adding a journal-type input to the create API. Not a service-layer gap. |

## Flagged uncertainties

- **Item close against a later company-wide recalculation.** Microsoft states only that "after you
  run closing, the system automatically cancels all recalculations for dates after the closing
  date" ([inventory costing FAQ](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-costing-faq)),
  with no rule for how that interacts with closing a single item. Company-wide closes already
  cancel later recalculations automatically, which matches the documented behavior. Cancelling
  only one item's share of a company-wide run would require partially cancelling a cost run, a
  shape D365 is not documented to have, so the item close keeps refusing and directs the
  operator to the documented **Reverse** action instead. Resolving this needs either a
  documented scoping rule or observation of a live D365 environment.

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

2026-11-11: **Scanned the inventory transaction (`InventTrans`) status-transition layer against
`CycleErpExamples/Inventory/Logic_Readonly.md` and fixed five value-allocation defects.** The status
machine itself (Ordered → Registered/Received → Purchased, Ordered → ReservPhysical → Picked →
Deducted → Sold, and their splits on partial quantities) was traced through every writer and is
sound, as is every `InventSum` bucket formula around it. What was wrong is the **money** carried on
the split and transitioned rows, in two forms.

*Rows that did not sum back to their own posted total.* `InventValueReportService` states the
contract the whole module relies on — a transaction is counted on the physical side or the financial
side, never both, and the invoice clears its physical cost — and reconciles the transaction-derived
value against `InventSum`. Four flows broke it by rounding each transaction's share independently
while the ledger entry and `InventSum` carried a single rounding of the total, leaving a permanent
cent-level `PhysicalValueDifference` in the value report (`REP-02`) whenever an invoice spanned more
than one movement: `VendorInvoiceService` (an unrounded `accountingAmount / inventQty` written into
each `decimal(18,4)` row, plus an unrounded per-unit physical release),
`SalesInvoiceService.IssueInventoryAsync` (per-layer `CostAmountPosted` against a once-rounded
`cogs`), `CustReturnService.ReceiptFinancialUpdateAsync` and `PurchReturnService` (per-unit physical
splits). All four now allocate cumulatively — the last row carries the remainder — and release the
*exact complement* of what each row recorded, which is the pattern `ProductReceiptService`,
`PackingSlipJournalService` and the multi-receipt settlement allocator already used.

*A physical value that was never maintained at all.* `PurchReturnService` wrote
`InventTrans.CostAmountPhysical` on the return shipment's `Deducted` rows but never moved
`InventSum.PhysicalValue`, and the credit note then promoted those rows to `Sold` without clearing
it. So a vendor return (`RET-03`) shipped stock out while its value stayed in physical inventory
value, and the two sides of the value report disagreed by the returned cost for as long as the
shipment was uncredited. Shipment now releases the shipped value and the credit hands it over to
`PostedValue`, symmetric with the packing-slip/sales-invoice pair.

Verified by a clean build and the existing suites: 147/147 `InventoryCycle.Tests` and 957/978
`IXApi.Tests` (21 pre-existing skips), with no new failures. These are value-reconciliation
corrections with no new tests of their own yet; a multi-movement partial invoice asserting
`sum(CostAmountPosted) == InventSum.PostedValue` and a shipped-not-credited vendor return asserting
a zero `PhysicalValueDifference` would prove them directly.

The reference file was renamed `Read_only.md` → `Logic_Readonly.md`; live pointers to it (this file,
the example package's README and documentation, the scenario README, the test comments) now use the
new name. Dated validation records keep the old name because that is what they checked.

2026-11-09: The user expanded Inventory acceptance to include batch/serial tracking and Include physical value. The latter was reproduced as setup rejection and a 10-unit provisional issue cost where the reference expects 15. Weighted-average setup and running-average calculation now include received/deducted physical balances when enabled; weighted-average closing remains financial-only. Retest evidence is tracked under `.validation/inventory-20261109`. Batch/serial remains unresolved and must not be counted as excluded or complete. The new opt-in Inventory example package follows the AP preparation/service-flow pattern; its initial executable scenario is the reference core cycle, not all acceptance cases.

| Date | Change |
| --- | --- |
| 2026-11-09 | **Made the inventory reference cycle executable, and it immediately found a blocking defect.** `CycleErpExamples/Inventory/` held only `Read_only.md` - no runnable flow and no validators, unlike the AP package - so the eleven-step example and its published figures were proven by nothing, which is why every case id in the reference read `Not run`. `Inventory_reference_cycle_closes_at_ninety_on_hand_and_one_thousand_eighty_value` in `Tests/InventoryCycleTests.cs` now runs all eleven steps through the **real** purchase, reservation, delivery, invoice and close services and asserts the reference figures: **90** on hand, cost of goods sold **720**, inventory value **1,080**, and the net cost adjustment of **120** split **-80** over the 40-piece issue and **-40** over the 20-piece issue. This matters because it proves the crux of periodic weighted average - the second receipt (50 at 16) arrives *after* both sales and still lifts the period average to 12, so already-invoiced issues are adjusted upward at close. **The costing arithmetic was correct**; 36 of the 37 existing close tests build `InventTrans` rows by hand, so this is the first time close has been proven after real document postings. **The defect it found: a non-stocked service line could be ordered and delivered but never invoiced.** `SalesInvoiceService` required an item model group for a non-stocked line and required that group to be non-stocked, while `InventModelGroupService` refuses to create any non-stocked model group - so both branches refused, with no group giving `Item model group is missing for {itemId}.` and any creatable group giving `Stocked product policy conflicts with sales line {recId}.` Step 9 of the reference, which bills a 300 service line beside the item, was therefore unreachable. `SalesInvoiceService` now treats a **`ItemType.Service` item with no model group as non-stocked**, which is exactly the rule `SalesInventoryDemandService` already applies when it declines to raise demand for the line; the two services had contradicted each other. Any non-service item with no model group is **still refused**, so an unconfigured stocked item cannot slip through. |

| Date | Change |
| --- | --- |
| 2026-11-05 | Moved the `Registered` / `Arrived` on-hand buckets from an open gap to *Out of scope*. They belong to the item arrival / production input journals, which D365 needs only when a model group sets registration or receiving requirements, and the basic model group validator refuses all four of those flags. Nothing is missing; the process that would write them is deliberately excluded. No behavior changed. |
| 2026-11-03 | Sales and purchase order cancellation now release open demand and ordered supply for a partly processed order, affecting `InventSum.Ordered`, `OnOrder` and `ReservPhysical` and the `Ordered`/`ReservPhysical` inventory transactions behind them. Neither posts a ledger entry or checks a fiscal period, because ordered supply and reservations are neither physical nor financial inventory. A **picked** quantity blocks sales cancellation by name, leaving the `Picked` bucket to the existing `SetPickedAsync` unpick path rather than reversing a physical registration inside cancellation. See `AccountsPayable/GAPS.md` and `AccountsReceivable/GAPS.md`. |
| 2026-11-03 | Marking removal can select an already-split movement by record ID. It refuses missing, ambiguous, or financially settled counterparts, and marking refuses a second origin pair that cannot be distinguished. Marking operations use the shared transaction lock. |
| 2026-11-03 | Customer return entitlement now distinguishes returns linked to other invoice lines from genuinely unattributed legacy returns, including lines added earlier in the same request. Legacy returns still trigger the conservative origin-wide limit. |
| 2026-11-03 | Verified the close/recalculation interaction against Microsoft: closing cancels all recalculations dated after the closing date, which the company-wide close already does automatically. No documented rule covers an item close against a later company-wide run, and scoping one would mean partially cancelling a cost run, so that case stays refused and moved from an open gap to a flagged uncertainty. No behavior changed. |
| 2026-11-03 | Sales return lines persist the invoice line they were created from in the new `SalesLine.OriginalInvoiceTransRecId`, a deliberate local extension symmetric with `PurchLine.OriginalInvoiceTransRecId`; `RefReturnInvoiceTransW` was left alone because the Common Data Model documents no purpose for it. Return creation, credit costing and inventory close now take the cost of that invoice instead of refusing an origin whose partial invoices carried different costs, and entitlement is counted against the selected invoice line so one partial invoice no longer consumes another's returnable quantity. The conservative origin-wide guard remains only while an unattributed legacy return exists. Migration `AddSalesLineOriginalInvoiceTrans`. |
| 2026-11-03 | Inventory close selects the original invoice before checking whether its issue is settled and before identifying same-period return dependencies. Other invoices on the same origin no longer block the selected invoice. Legacy returns without an invoice link retain the ambiguity guard. |
| 2026-11-03 | FIFO close can settle same-period sales returns when later non-return receipts exist, provided earlier receipts cover the issue prefix. An insufficient earlier pool is still refused without settling later receipts ahead of the return. |
| 2026-11-03 | Weighted-average close carries remaining value forward using active financial settlement costs, verifies the recorded receipt quantities, and excludes cancelled settlements. Marked or purchase-return settlement against an ambiguous original layer is refused. |
| 2026-11-03 | Weighted-average close now handles wholly marked issues alongside unmarked issues when the entire unmarked pool is settled, excluding the marked receipt quantity and value from the average. Mixed pools with remaining stock remain refused pending carry-forward support. |
| 2026-11-03 | FIFO close can settle a same-period original sales issue before costing its return in a simple, chronologically ordered pool; failure to settle the original issue rolls back the close. |
| 2026-11-03 | Purchase-return cost adjustments are implemented instead of refused. A vendor credit note now records its financial `InventTransPosting` pair, and inventory close reuses the recorded pair for every source that has one. Where no record exists, the close resolves the pair of the cycle the transaction belongs to: per Microsoft the Inventory tab accounts apply only to transactions unrelated to sales, purchases or production, so a purchase-related issue uses "Cost of purchased materials invoiced" offset by "Purchase expenditure for product" ([purchase order posting](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/purchase-order-posting), [inventory posting](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/inventory-posting)). Journal, counting and sales sources still refuse a missing record rather than approximating it. `PurchConsump` is now seeded (`60018000`) and required by company and item readiness, since a financial purchase posting cannot resolve without it. |
| 2026-11-03 | Customer return creation conservatively counts all prior returns against the selected invoice line so separate requests cannot exceed its quantity while the original invoice relationship is unavailable. |
| 2026-11-03 | Vendor returns resolve the posted invoice from its selected line and value shipment, credit and close using that invoice's inventory movements. Ambiguous legacy costs and non-unique invoice-line movements are refused. |
| 2026-11-03 | Product receipts translate foreign-currency purchase values at the receipt date before recording physical inventory value, including when physical ledger accrual is disabled. Missing rates are refused. Foreign-currency physical accrual posting remains unsupported. |
| 2026-11-03 | Purchase invoice accrual reversal selects the physical posting by inventory origin, receipt voucher and item, and rejects ambiguous or inconsistent posting accounts. |
| 2026-11-03 | Partial sales invoices preserve the remaining delivery's physical cost and reverse physical ledger postings by packing slip using its original accounts. A posting stays active until its delivery is fully invoiced. |
| 2026-11-02 | Counting confirmation validates the item's current inventory unit and configured decimal precision, and explicitly refuses values beyond the four-decimal inventory storage limit. |
| 2026-11-02 | FIFO/LIFO close reserves only the open quantity of a wholly marked issue, allowing other issues to consume excess from its receipt while preserving the marked quantity. |
| 2026-11-02 | FIFO/LIFO close protects marked receipt layers from competing issues; ambiguous partial or split markings, mixed marked/unmarked weighted-average issues, and pools mixing purchase returns with marking are refused. |
| 2026-11-02 | Marking rejects different or missing financial sites. Close refuses a marked issue when its marked receipt has no available financial quantity in the item/site pool instead of silently substituting another receipt. Split movement selection and protection of marked layers from competing issues remain open. |
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
