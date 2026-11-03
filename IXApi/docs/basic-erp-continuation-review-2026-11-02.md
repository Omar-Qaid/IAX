# Basic ERP continuation review — 2026-11-02

Status: **not complete**. This review follows the attached inventory-first scope. The attachment explicitly requests analysis only until implementation is authorized. No application code, schema, configuration or business data was changed in this review. Pre-existing edits in `InventoryArchitectureTests.cs` and `InventTransferOrderService.cs` were preserved.

## Current evidence

| Check | Result |
| --- | --- |
| `dotnet build IXApi/IXApi.csproj --no-restore -v:q` | Passed, zero errors; existing package advisory warnings. |
| `dotnet test IXApi/Tests/InventoryCycle.Tests.csproj --no-restore -v:q` | 31 passed, zero failed. SQLite fixtures, not SQL Server acceptance. |
| `dotnet test IXApi/Tests/IXApi.Tests.csproj --no-restore -v:q` | 694 passed, 3 failed, 2 skipped. |
| Focused rerun of the three failing GL tests | All three reproduced. Two reversals report `Journal name 'GEN' has no voucher number sequence.` Allocation-at-posting fails at `LedgerJournalPostingTests.cs:86` because the fixture's journal-name query returns no row. These are baseline failures observed before any source edits in this pass; production impact is not established. |
| Target SQL Server, deployed setup and business UI acceptance | Not verified in this pass. Earlier migration-success claims remain historical evidence only. |

The passing cycle fixture covers reservations, partial purchase receipts/invoices, partial sales delivery/invoicing/payment, customer/vendor returns, currency translation, FIFO settlement, and cancellation of a wholly unprocessed sales order. It does not prove every allowed business configuration or runtime database behavior.

## Minimum gap register

Paths below are relative to `IXApi/src/Modules/Finance/` unless otherwise stated.

| Priority / process | Status and source evidence | Verified expectation / inconsistency | Dependencies and minimum correction |
| --- | --- | --- | --- |
| P1 verification prerequisite: configured GL vouchers | Not verified by the failing tests. `IXApi/Tests/LedgerJournalPostingTests.cs:86,256,491`; reversal rejects missing journal voucher setup. | A refusal for absent setup must not be bypassed with a generated constant. The tests currently fail before proving configured voucher allocation and reversal. | Inspect and supply the fixture's real journal-name/number-sequence dependencies, then rerun these three tests. Separately check actual company setup; a fixture failure does not establish missing production configuration. |
| P2 purchase integration: cancel unreceived remainder | Missing in inspected API/service. `AccountsPayable/PurchaseOrder/Controllers/PurchTableController.cs` offers create, read, add line, confirm, receive and invoice. Generic update/delete are disabled. `IPurchOrderService` has no cancellation method. Repository-wide cancellation searches found no separate purchase cancellation execution path. | Microsoft permits canceling only the remainder that has not been registered, received or invoiced. The received portion remains processable. [Purchase-order cancellation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/purchase-order-approval-confirmation). | Reuse purchase line/origin/ordered-supply records. Add one company-scoped transactional action that reduces only open supply and the corresponding `InventSum.Ordered`, updates commercial/inventory remainders and derives statuses while preserving receipt/invoice links. Keep received-but-uninvoiced quantities invoiceable. No new convenience table or GL voucher for mere cancellation of ordered supply. |
| P2 sales integration: cancel undelivered remainder | Partially implemented. `AccountsReceivable/SalesOrder/Services/SalesOrderCancellationService.cs:38-43` rejects any line that is processed; `SalesTableController.cs:184-194` exposes that restriction. `SalesOrderLineService.ChangeAsync` also rejects processed lines, so normal line editing is not a remainder workaround. | Partial fulfillment must preserve delivery and invoice remainders separately. Microsoft documents that the sales order reaches Invoiced when both remainders are zero. [Customer invoicing](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/configure-customer-invoices). | Extend cancellation deliberately; do not merely remove its guard. `SalesInventoryDemandService.CancelRemainingAsync` currently zeros *all* physical and financial remainders. Preserve delivered-but-uninvoiced quantities, release only remaining ordered/reserved demand, and derive line/header statuses. Picked quantities require explicit treatment through the existing unpick path. |
| P1 release verification: inventory runtime | Not verified on the intended SQL Server. Current cycle tests use SQLite. `Inventory/GAPS.md` records migration application but still flags concurrent posting and legacy transfer orders. | Source-code and SQLite success do not certify database locking, atomic rollback, number allocation or deployed company setup. This is an evidence gap rather than an invented D365 feature. | Use an approved nonproduction target for a focused ordinary cycle and duplicate-post/rollback check. Confirm legacy transfer data before choosing a compatibility correction; do not guess a backfill. Avoid an exhaustive test matrix. |
| P2 business UI completion | Partially implemented / full usability not verified. Inventory setup and journal APIs/pages are present, including posting and count freeze/confirm. `IXApp/src/modules/finance/inventory/routes/inventoryRoutePaths.ts` has no close or transfer-order route; repository searches found no typed clients named for those processes. The finance frontend has no dedicated accounts-payable module directory. Generic metadata-driven coverage is not verified. | An API implementation alone does not establish that an operator can complete the agreed business cycle. | Trace generic route/metadata support before declaring an entire screen missing. Expose only the essential transaction actions that have no usable existing route. Do not add reports or dashboards. |
| P1 GL follow-on: basic year-end | Partially implemented; in scope because the user's attachment explicitly includes it. `GeneralLedger/FiscalCalendar/Controllers/LedgerYearEndCloseController.cs` contains the posting process and constructs posted-entry headers directly. It selects a next-year period with an Opening preference but accepts another type as fallback. | Microsoft describes opening balances, retained earnings and reruns; it requires a supplied voucher, while the current request has no voucher field and derives `YEAR-OPEN-*` identifiers. [Year-end close](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/year-end-close). Exact target-period eligibility remains to be verified; ordinary operating-period posting rules must not be copied blindly onto opening transactions. | After inventory and order dependencies, move only this process into the existing service/interface architecture and use shared posting construction with verified year-end rules. Add explicit voucher input and check supported rerun behavior. Preserve dimensions according to the basic chosen policy. Do not remove year-end as an alleged advanced feature. |

Example: order 10, receive/deliver 6, invoice 4, then cancel the unprocessed 4. The two already received/delivered units awaiting invoice must remain invoiceable; stock and previously posted accounting must remain intact. Existing sales cancellation safely refuses this case, but therefore does not complete the agreed basic cycle.

## Scope and record corrections

- No additional costing model, batch/serial dimension, arrival-registration workflow, partial marking feature, reports, or advanced modules is required by this request. Conditional items in older gap lists are not automatic implementation work.
- `IXApp/.../Basic-Inventory-Implementation-Checklist.md` dates from 2026-10-23. Its Missing/Blocked claims for purchase, returns and costing are historical and contradicted by newer passing backend cycle tests. Its frontend acceptance claims must still be checked separately.
- The finance gap analysis contains later sections recording pending vendor invoices, payment/settlement reversals and cash discounts as implemented. Do not rebuild them based on the earlier open register.
- The user's explicit basic year-end requirement overrides the older repository/skill exclusion. Financial dimension derivation remains excluded.

## Continuation order

1. Resolve the three voucher-fixture failures without weakening required setup validation; verify actual deployment setup separately.
2. Complete purchase and sales remainder cancellation as inventory dependencies, preserving physical/financial remainders.
3. Verify the focused inventory cycle on the approved nonproduction SQL Server, including transfer compatibility where old orders exist.
4. Trace existing generic UI coverage, then fill only essential operator-flow gaps.
5. Continue the remaining sales, purchasing and GL assessment, including basic year-end. Areas not traced in this pass remain **Not verified**, not complete.

This is a continuation checkpoint and written gap analysis, not certification that the entire ERP scope is finished.
