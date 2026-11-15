# Core ERP cycle — QA execution report (PlanTestForFullEmaple.md)
> **Latest verified result — SQL enabled:** Main suite **1,150 passed, 0 failed, 0 skipped**; inventory suite **147 passed, 0 failed, 0 skipped**. All original 2 failures and 21 skips passed in the same final main-suite run. Evidence: `TestResults/FullExampleEnabled/20261115-145803/`. Earlier runs below are retained as history and are superseded by the SQL-enabled follow-up at the end.

**Earlier run date:** 2026-11-15 · **Branch:** `feat/ap-pending-vendor-invoice` · **Database:** isolated in-memory SQLite per test (never the hosted database) · **Entry points:** the real Finance services, invoked by `IXApi/Tests/FullExampleCycleTests.cs` and `IXApi/Tests/InventoryWeightedAverageReturnCloseTests.cs` · **Earlier suite:** `dotnet test Tests/IXApi.Tests.csproj` — 1128 passed, 0 failed, 21 skipped.

The integrated cycle below is executed by one ordered test,
`Core_erp_reference_cycle_closes_at_fifty_six_pieces_and_a_profit_of_nineteen_hundred`, whose
asserts are the database evidence: every checkpoint reads `InventTrans`, `InventSum`, `PurchLine`,
`SalesLine`, `VendTrans/Open`, `CustTrans/Open`, `BankAccountTrans`, `InventSettlement` and
`GeneralJournalAccountEntry` directly, never an API status alone.

> **Current revalidation:** The earlier results below are retained as history. See the revalidation appendix for this task's commands, newly exposed fixture defects, individual action coverage, and current verification limits.
## Integrated cycle results

| Test ID | Phase | Entry point | Expected | Actual | Status |
|---|---|---|---|---|---|
| GL-01 | 01/10 | `LedgerJournalPosting` | Bank 10,000 against capital | 10,000 in `BankAccountTrans`, voucher balanced | Passed |
| PO-01..04 | 02 | `PurchOrderService`, `PurchConfirmationService`, `ProductReceiptService`, `VendorInvoiceService` | 60/40 split: remainders 40/100 → 40/40 → 0/0, accrual 3,000 then 0, vendor 5,000 | As expected (`PurchLine.RemainPurchPhysical/Financial`, `InventTrans` Received→Purchased, accrual pair 130110/210110 flat) | Passed |
| PO-05, AP-01 | 03 | `VendorInvoiceService` (service PO), `LedgerJournalPosting` (direct 80) | Expenses 200 + 80, no stock impact | Maintenance 200, AdminExpense 80, no `InventTrans` for SRV-MAINT | Passed |
| TR-01 | 04 | `InventTransferOrderService` | A=70, B=0→30, TRANSIT=30→0, value 5,000 | As expected (`InventSum` per warehouse) | Passed |
| SO-01..04 | 05 | demand + confirmation + `InventoryReservationService` + `PackingSlipJournalService` + `SalesInvoiceService` | Reserve 40 (avail 30), 25 then 15+service; AR 4,300; COGS 2,000 | As expected (`SalesLine.RemainSalesPhysical/Financial`, delivery pair 130120/510110 flat after invoicing) | Passed |
| AR-01..02, AP-02 | 06 | `FreeTextInvoiceService`, `PurchReturnService`, `CustReturnService` | FTI 100; vendor return −5 pcs/−250; customer return +2 pcs/+100 cost; credit-only −50 | As expected; credit-only modelled as 0.5 qty of the 100-price line (`CustReturnService` has no amount-only credit — see Flagged) | Passed |
| AP-03..04 | 07 | `VendPaymentService`, `VendSettleOpenTransactionsService` | First invoice closed; second open 750; AP 1,030 | As expected (`VendTransOpen`, `VendSettlement`) | Passed |
| AR-03..05 | 08 | `CustPaymentService`, `CustSettleOpenTransactionsService` | First invoice closed; second open 1,050 + FTI 100; AR 1,150 | As expected (`CustTransOpen`, `CustSettlement`) | Passed |
| INV-08..09 | 09 | `InventJournalService` (counting), `InventCloseService` | −1 piece, 50 loss; close settles; stock A=27, B=29; value 2,800 | **Failed → Fixed → Passed** (see defect 1) | Passed |
| GL-03..07 | 10 | `LedgerJournalPosting`, `PostedBalancesAsync` trial balance | Bank 8,980; AP 1,030; AR 1,150; inventory 2,800; revenue 4,150; profit 1,900; held period refused | As expected; trial balance 15,180 = 15,180; OnHold period refused with "The fiscal period for {date} is not open." | Passed |

**Final reconciliation: 56 pieces, inventory 2,800, AP 1,030, AR 1,150, bank 8,980, profit 1,900 — full cycle Passed.**

## Standalone tests (plan §3)

| Case | Coverage | Status |
|---|---|---|
| Full/partial processing and cancellation | Existing: `Cancelling_a_partly_received_order_releases_the_remainder_and_keeps_the_receipt_invoiceable`, `Cancelling_a_partly_delivered_order_…`, `Cancellation_releases_split_reserved_and_ordered_demand_…` | Passed (pre-existing) |
| Invoice matching over-receipt rejection | Existing: `VendInvoiceInfoTests` — "Invoice quantity … exceeds the received, uninvoiced quantity" plus price/tolerance matching tests; nothing posts on rejection | Passed (pre-existing) |
| Stock controls | Existing: `RES02_reserving_beyond_demand_or_beyond_available_stock_is_refused`, counting-journal generation/confirmation tests, movement and adjustment journal posting tests | Passed (pre-existing) |
| Weighted average (10×40, sell 5, 10×60 → 200 → 250, 15 @ 750) | **New:** `Standalone_weighted_average_close_moves_the_issue_from_two_hundred_to_two_hundred_fifty` in `FullExampleCycleTests.cs` — end to end through real PO/SO services | Passed |
| Corrections (unsettle/re-settle, payment reversal, GL reversal) | Existing: `LedgerSettlementTests`, payment-reversal tests (`A_customer_payment_with_a_bank_offset_posts_and_reverses_on_both_subledgers`, `A_settlement_cannot_be_reversed_twice`, …), journal reversal tests | Passed (pre-existing) |
| Tax 15% | Existing: `TaxSettlementTests`, `Free_text_invoice_posts_services_tax_and_receivable_without_inventory`, purchase tax posting tests | Passed (pre-existing) |

## Defects found and fixed

1. **Weighted-average close refused a same-period customer return** — *Failed → Fixed → Passed.*
   Root cause: only FIFO pre-settled the original sales issue before costing its return;
   `InventCloseService` refused the pool with "A customer return requires its original financially
   settled sales issue in the same pool." Fix: a weighted-average pre-pass (close only, no marked
   issues) settles the pool without the dependent return receipts, then costs the returns at the
   issue's settled unit cost; the history check also reads pending settlements. Evidence:
   `InventoryWeightedAverageReturnCloseTests` (adjustments both directions, later-receipt average,
   cancel symmetry, recalculation refusal) and the integrated INV-09 close. Recorded in
   `Inventory/GAPS.md` (2026-11-15).

## Flagged (not defects)

- **Credit-only amount:** `CustReturnService` has no amount-only credit note; the 50 credit is a
  0.5-quantity credit of the 100-price line. D365's credit-only disposition is quantity-based too,
  but a pure amount adjustment would go through a free text credit; recorded in the test.
- **Transfer legs after close:** transfer-order legs stay `ValueOpen = Yes` by design — close
  verifies them as balanced internal movements and settles only sourced issues.

## Reference name mapping

Project tables match the D365 reference names (`PurchTable/Line`, `SalesTable/Line`, `InventTrans`,
`InventTransOrigin`, `InventSum`, `InventSettlement`, `VendTrans/Open`, `CustTrans/Open`,
`VendSettlement`, `CustSettlement`, `BankAccountTrans`, `GeneralJournalEntry`,
`GeneralJournalAccountEntry`, `LedgerJournalTable/Trans`). The ledger journal header entity class
is `LedgerJournalTable` (aliased `JournalHeader` in tests). Confirmation journals are
`VendPurchOrderJour`, `CustConfirmJour/Trans`; invoices `CustInvoiceJour`, `VendInvoiceJour`.

## Revalidation — 2026-11-15

**Earlier SQLite revalidation result:** the repaired reference cycle and return-costing checks passed 5/5; the inventory suite passed 147/147; the API build passed with dependency rebuild disabled. Broad regression execution is recorded separately below. No application-service defect was reproduced; the changes repair fixture correctness and acceptance coverage.

The four acceptance references were read before execution. The checkout started clean. No application database, migrations, production configuration, or application service source was changed. Each cycle test creates and disposes its own `Data Source=:memory:` SQLite connection and seeds company `dat`; identifiers are isolated by that connection. The source fixture uses simplified EF mappings, so these results do not establish SQL Server constraints, application DI/API routing, frontend behavior, or cross-connection locking.

### Repairs and failure history

1. **Sales creation bypass — Failed → Fixed → Passed (final focused rerun).** Both reference tests inserted `SalesTable` and `SalesLine` directly and set `StockedProduct` themselves. They now use `SalesOrderQuickCreateService.CreateAsync`, `SalesOrderHeaderService.UpdateAsync`, and `SalesOrderLineService.AddAsync`. The normal line entry point rejected both tests with **"A sales tax group is required for the sales line."** Evidence: `TestResults/FullExampleReview/strengthened-cycle.trx`. The fixture now supplies explicit zero-rated tax groups, their common code and effective zero rate, and inherits the groups through the customer and item setup. This preserves the reference's no-tax balances without bypassing validation.
2. **Incomplete service setup — repaired.** `SRV-01` lacked its inventory-unit configuration and non-stocked model-group association. Both are now configured, and the tests assert the stocked item and service flags produced by the sales-line service.
3. **Payment-term coverage — repaired.** The vendor lacked `NET30`. The fixture now sets it and checks both `VendTrans/Open` due dates at invoice date + 30 days; customer due dates receive the same check.
4. **Missing rejection/open-payment evidence — repaired.** The cycle now attempts 61 against 60 received, verifies 422 and no invoice/party/voucher writes or remainder change, then posts the corrected 60 using the same invoice reference. It also checks the separate 1,000 vendor/customer payments are individually open before settlement, and that the credit-only posting adds no inventory transactions.
5. **Test selection — corrected.** `InventoryWeightedAverageReturnCloseTests.cs` declares a partial `InventoryCycleTests` class. Filtering by the filename does not select those tests. The three return-costing cases were selected by their actual method names and passed.

### Commands and evidence

All commands run from the repository root. `--no-restore` uses existing package assets. The sandboxed .NET build/test attempts exited 1 without actionable diagnostics; the authorized external execution successfully compiled Finance and executed the inventory suite. A dependency rebuild remained in Infrastructure compilation and was stopped as a duplicate once the separate test build was available. Tests against existing unchanged application assemblies use `-p:BuildProjectReferences=false -p:UseSharedCompilation=false`; this limitation is explicit, rather than claiming a fresh build of every dependency.

| Run | Command / selection | Result | Evidence under `TestResults/FullExampleReview/` |
|---|---|---|---|
| Existing-binary reference baseline | `dotnet vstest IXApi/Tests/bin/Debug/net9.0/IXApi.Tests.dll` with `FullExampleCycleTests` filter | Passed: 2 | `prechange-binary-baseline.trx` |
| Existing-binary return-costing baseline | Same binary; `Weighted_average_close_settles_same_period` and `Weighted_average_recalculation_still_refuses` filters | Passed: 3 | `return-costing-baseline.trx` |
| Required inventory regressions | `dotnet test IXApi/Tests/InventoryCycle.Tests.csproj --no-restore` | Passed: 147, failed: 0, skipped: 0 | `inventory-cycle.trx` |
| First strengthened cycle | Main test project, no dependency rebuild, `FullExampleCycleTests` filter | Failed: 2, fixture tax setup missing | `strengthened-cycle.trx` |
| Final focused rerun | `dotnet vstest IXApi/Tests/bin/FullExampleReview/IXApi.Tests.dll` with the actual cycle and return-costing method filters | Passed: 5, failed: 0, skipped: 0 | `cycle-final.trx` |
| Main regression suite before tax-setup repair | Main project, no dependency rebuild, all tests | 1,127 passed; 2 fixture failures; 21 skipped; 1,150 total | `full-suite-current.trx` |
| API build | `dotnet build IXApi/IXApi.csproj --no-restore -p:BuildProjectReferences=false -p:UseSharedCompilation=false` | Passed: 0 errors; 6 existing package-audit warnings | Terminal build result |

### Individual integrated actions

These rows map each action to the database assertions in `FullExampleCycleTests.Core_erp_reference_cycle_closes_at_fifty_six_pieces_and_a_profit_of_nineteen_hundred`. The final five-case focused rerun passed; the actual value for each integrated action matched the stated expectation. These are assertions inside one ordered cycle test, rather than 57 independently discoverable tests. Receipt/invoice references below are explicit request identifiers; services allocate PO/SO numbers and sales invoice numbers, so use the persisted source references rather than assuming the comments' illustrative `PO-001`/`SO-001` are actual sequence values. A posted sales invoice is looked up through `CustInvoiceJour.ParmId` in the test. Voucher balance verification joins account entries to their actual `GeneralJournalEntry.RecId`.

| Test ID | Phase | Service / entry point | Input | Expected | Actual | Tables and fields verified | Document / voucher reference | Status | Evidence |
|---|---|---|---|---|---|---|---|---|---|
| R-01 | 01 | LedgerJournalPosting | Opening 10,000 | Bank 10,000; capital −10,000 | Matches expected | BankAccountTrans.AmountCur; GL signed amounts | GJ-OPEN / GJV-0001 | Passed | Final cycle TRX; GL-01 checkpoint |
| R-02 | 02 | PurchOrderService.CreateAsync | V-001, SAR, A | Open PO | Matches expected | PurchTable | Allocated PurchId | Passed | PO-01 |
| R-03 | 02 | PurchOrderService.AddLineAsync | 100 × 50 | Supply 100; stock 0 | Matches expected | PurchLine remainders; InventTrans.Qty/StatusReceipt | PO line | Passed | PO-01 |
| R-04 | 02 | PurchConfirmationService.ConfirmAsync | PO | Confirmation; no liability | Matches expected | VendPurchOrderJour; VendTrans | PO confirmation | Passed | PO-01 |
| R-05 | 02 | ProductReceiptService.PostAsync | 60 | Stock 60; remainder 40/100; accrual 3,000 | Matches expected | PurchLine; InventTrans; InventSum; GL | PR-001 | Passed | PO-02 |
| R-06 | 02 | VendorInvoiceService.PostAsync | 61 vs 60 | 422; no financial writes | Matches expected | VendInvoiceJour/Trans; VendTrans/Open; GJE count; remainders | VI-001 rejected attempt | Passed | Added overReceipt checkpoint |
| R-07 | 02 | VendorInvoiceService.PostAsync | Correct to 60 × 50 | AP 3,000; remainder 40/40; due +30 days | Matches expected | Purchased InventTrans; VendTrans/Open.DueDate; GL | VI-001 | Passed | PO-03 |
| R-08 | 02 | ProductReceiptService.PostAsync | Remaining 40 | Stock 100; remainder 0/40 | Matches expected | PurchLine; InventSum; accrual GL | PR-002 | Passed | PO-04 |
| R-09 | 02 | VendorInvoiceService.PostAsync | 40 × 50 | AP 5,000; remainder 0/0; accrual 0 | Matches expected | PurchLine.PurchStatus; InventTrans; GL | VI-002 | Passed | PO-04 |
| R-10 | 03 | PurchOrderService.CreateAsync | Service vendor order | Separate PO | Matches expected | PurchTable | Allocated service PurchId | Passed | PO-05 |
| R-11 | 03 | PurchOrderService.AddLineAsync | Maintenance 200 | No stock movement | Matches expected | PurchLine; absent service InventTrans | SRV-MAINT | Passed | PO-05 |
| R-12 | 03 | PurchConfirmationService.ConfirmAsync | Service PO | Confirmed | Matches expected | Service result; later invoice | Service PO | Passed | PO-05 |
| R-13 | 03 | ProductReceiptService.PostAsync | Service acceptance 1 | Accrual remains zero | Matches expected | GL accrual; no stocked transaction | SR-001 | Passed | PO-05 |
| R-14 | 03 | VendorInvoiceService.PostAsync | Service 200 | Maintenance 200; AP +200 | Matches expected | GL expense/AP; InventSum value | VI-S01 | Passed | PO-05 |
| R-15 | 03 | LedgerJournalPosting | Vendor expense 80 | Admin expense 80; AP +80 | Matches expected | VendTrans/Open; GL | VI-N01 / GJV-0002 | Passed | AP-01 |
| R-16 | 04 | InventTransferOrderService.CreateAsync | 30 A→B via transit | A100/B0/T0 | Matches expected | InventSum.PhysicalInvent | TR-001 | Passed | INV-02 |
| R-17 | 04 | InventTransferOrderService.ShipAsync | 30 | A70/B0/T30 | Matches expected | InventSum by warehouse | TR-001-SHIP | Passed | INV-03 |
| R-18 | 04 | InventTransferOrderService.ReceiveAsync | 30 | A70/B30/T0; value 5,000 | Matches expected | InventSum quantities/value; GL inventory | TR-001-RECV | Passed | INV-04 |
| R-19 | 05 | SalesOrderQuickCreateService.CreateAsync | C-001, SAR, A | Order created | Matches expected | Persisted SalesTable | Allocated SalesId | Passed | CreateSalesOrderAsync |
| R-20 | 05 | SalesOrderHeaderService.UpdateAsync | Order date; NET30 | Header saved | Matches expected | SalesTable; later invoice due date | Sales header | Passed | CreateSalesOrderAsync |
| R-21 | 05 | SalesOrderLineService.AddAsync | Stocked 40 × 100 | Stocked flag; demand −40 | Matches expected | SalesLine.StockedProduct; InventTrans | ITEM-01 | Passed | SO-01 |
| R-22 | 05 | SalesOrderLineService.AddAsync | Non-stocked service 300 | No stock demand; order total 4,300 | Matches expected | SalesLine; SalesTable.SmmSalesAmountTotal | SRV-01 | Passed | SO-01 |
| R-23 | 05 | SalesOrderConfirmationService.PostAsync | Two lines | 1 confirmation/2 lines; AR 0 | Matches expected | CustConfirmJour/Trans; CustTrans | CONF-001 | Passed | SO-01 |
| R-24 | 05 | InventoryReservationService.ReserveAsync | 40 | Physical70; available30 | Matches expected | InventSum.PhysicalInvent/AvailPhysical | Stocked sales InventTransId | Passed | Reservation checkpoint |
| R-25 | 05 | PackingSlipJournalService.PostAsync | 25 | A45; remainder15/40; delivered cost1,250 | Matches expected | Deducted InventTrans; SalesLine; GL | PS-001 | Passed | SO-02 |
| R-26 | 05 | SalesInvoiceService.PostAsync | 25 × 100 | AR2,500; COGS1,250; remainder15/15 | Matches expected | Sold cost; CustTrans/Open; GL; due dates | SI-001 request | Passed | SO-03 |
| R-27 | 05 | PackingSlipJournalService.PostAsync | Last15 | A30 | Matches expected | InventSum.PhysicalInvent | PS-002 | Passed | SO-04 |
| R-28 | 05 | SalesInvoiceService.PostAsync | 15 + service300 | Invoice1,800; remainder0/0; COGS2,000 | Matches expected | SalesLine; CustTrans; GL account allocation | SI-002 request | Passed | SO-04 |
| R-29 | 06 | FreeTextInvoiceService.CreateAsync | Service100 | Draft invoice | Matches expected | Returned draft reference | Allocated FTI RecId | Passed | AR-01 |
| R-30 | 06 | FreeTextInvoiceService.PostAsync | Draft | AR+100; service revenue+100; stock unchanged | Matches expected | CustTrans; GL; InventSum | Allocated FTI InvoiceId | Passed | AR-01 |
| R-31 | 06 | PurchReturnService.CreateAsync | 5 against VI-002 | Return order/line | Matches expected | PurchTable.PurchaseType; PurchLine.PurchQty | RMA-V-001 | Passed | AP-02 |
| R-32 | 06 | PurchReturnService.ShipAsync | 5 | A25; AP unchanged | Matches expected | InventSum; VendTransOpen | RS-001 | Passed | INV-06 |
| R-33 | 06 | PurchReturnService.CreditAsync | Credit250 | AP−250; inventory−250 | Matches expected | VendTrans/Open; InventSum; GL | VCN-001 | Passed | AP-02 |
| R-34 | 06 | CustReturnService.CreateAsync | 2 against original invoice | SalesQty−2 | Matches expected | SalesLine.SalesQty | Physical return SalesId | Passed | AR-02 |
| R-35 | 06 | CustReturnService.ReceiveAsync | 2 | A27; receipt+2 | Matches expected | InventTransOrigin; InventTrans.Qty | RR-001 | Passed | INV-07 |
| R-36 | 06 | CustReturnService.CreditAsync | Credit200 | AR−200; COGS−100; inventory+100 | Matches expected | CustTrans; InventSum; GL | CN-001 request | Passed | AR-02 |
| R-37 | 06 | CustReturnService.CreateAsync | Credit-only0.5 × 100 | Separate return | Matches expected | SalesTable.SalesType | Credit-only SalesId | Passed | AR-03 |
| R-38 | 06 | CustReturnService.CreditAsync | Credit-only50 | AR−50; no movement/cost change | Matches expected | InventTrans count; CustTrans; GL | CN-002 request | Passed | AR-03 |
| R-39 | 07 | VendPaymentService.PostAsync | 3,000 marked to VI-001 | First invoice closed; bank7,000 | Matches expected | VendTransOpen.RefRecId; GL bank | PAY-001 | Passed | AP-03 |
| R-40 | 07 | VendPaymentService.PostAsync | Unmarked1,000 | Individual payment remains open | Matches expected | VendTransOpen.AmountCur=+1,000 | PAY-002 | Passed | AP-04 |
| R-41 | 07 | VendSettleOpenTransactionsService.SettleAsync | Payment+credit vs VI-002 | VI-002 open750; total AP1,030 | Matches expected | VendTransOpen by RefRecId; GL | Three selected VendTrans RecIds | Passed | AP-04 |
| R-42 | 08 | CustPaymentService.PostAsync | 2,000 marked | SI-001 open500; bank8,000 | Matches expected | CustTransOpen; GL | RCPT-001 | Passed | AR-04 |
| R-43 | 08 | CustPaymentService.PostAsync | Unmarked1,000 | Individual receipt remains open | Matches expected | CustTransOpen.AmountCur=−1,000 | RCPT-002 | Passed | AR-05 |
| R-44 | 08 | CustSettleOpenTransactionsService.SettleAsync | Allocate500 to each invoice | SI-001 closes; SI-002 open1,300 | Matches expected | CustTransOpen by RefRecId | Receipt and two invoice RecIds | Passed | AR-05 |
| R-45 | 08 | CustSettleOpenTransactionsService.SettleAsync | Credits200+50 | SI-002 open1,050; FTI100; total1,150 | Matches expected | CustTransOpen; GL AR | Credit and invoice RecIds | Passed | AR-05 |
| R-46 | 09 | InventJournalService.CreateAsync | Count B | Counting journal created | Matches expected | Returned JournalId | Allocated COUNT JournalId | Passed | INV-08 |
| R-47 | 09 | InventJournalService.AddLineAsync | Count29 vs system30 | Saving leaves B30 | Matches expected | InventSum.PhysicalInvent | Counting line | Passed | INV-08 |
| R-48 | 09 | InventJournalPostingService.FreezeCountAsync | Count journal | Freeze accepted | Matches expected | Service result; subsequent count | Counting journal | Passed | INV-08 |
| R-49 | 09 | InventJournalPostingService.ConfirmCountAsync | Count29 | Count confirmed | Matches expected | Service result; posted quantity | Counting line RecId | Passed | INV-08 |
| R-50 | 09 | InventJournalPostingService.PostAsync | Difference−1 | B29; loss50; inventory2,800 | Matches expected | InventJournalTrans.Qty; InventSum; GL | Counting voucher | Passed | INV-08 |
| R-51 | 10 | LedgerJournalPosting | Bank expense20 | Bank8,980; expense20 | Matches expected | BankAccountTrans; GL | GJ-BANK / GJV-0003 | Passed | GL-03 |
| R-52 | 09 | InventCloseService.CloseAsync | November30 | No cost difference; settled sales issues | Matches expected | InventTrans adjustment/ValueOpen; close result | Allocated closing reference | Passed | INV-09 |
| R-53 | 09 | InventValueReportService.RunAsync | November30 | 56pcs; value2,800; GL agrees | Matches expected | InventSum; value report; GL | November value report | Passed | INV-09 |
| R-54 | 10 | LedgerTrialBalanceService.GetAsync | November | Debit=credit15,180; profit1,900 | Matches expected | GJE/GJAE; control-account balances | November trial balance | Passed | GL-02/04/05/06/07 |
| R-55 | 10 | LedgerFiscalCalendarPeriodService.SetStatusAsync | OnHold | Status updated | Matches expected | LedgerFiscalCalendarPeriod | November period1 | Passed | GL-07 |
| R-56 | 10 | LedgerJournalPosting | Held November date | Rejected; balances unchanged | Matches expected | November posted GL before/after | GJ-LATE / GJV-0004 | Passed | GL-07 |
| R-57 | 10 | LedgerJournalPosting | Open December date | Posted10; November unchanged | Matches expected | GL by accounting date | GJ-DEC / GJV-0005 | Passed | GL-07 |

### Scope limits

These are service-level isolated tests. The fixture's seed configuration is inserted directly, and number sequences use a test implementation. Passing them does not verify the normal configuration/master-data CRUD entry points, actual number-sequence concurrency, complete production EF mappings/constraints, every source-document field listed in the plan, all posting dimensions, or SQL Server behavior. Some individual actions assert the service result followed by downstream database checks rather than an independent database snapshot immediately after the action. The tables above state exactly which checks exist. Such limits must not be presented as full API/UAT coverage.

**Current integrated reconciliation (final focused rerun):** stock `A=27`, `B=29`, `TRANSIT=0`; 56 pieces; inventory 2,800; AP 1,030; AR 1,150; bank 8,980; net revenue 4,150; COGS 1,900; counting loss 50; other expenses 300; profit 1,900. Trial-balance debit and credit balances are both 15,180; balance-sheet assets and liabilities/equity including profit are both 12,930. November OnHold rejects posting and December remains available.

The custom-output build compiled the corrected test assembly but could not copy project dependencies because the global OutputPath also redirected their expected locations. The final run therefore copied the unchanged runtime dependencies from `Tests/bin/Debug/net9.0` and the freshly compiled **implementation** assembly/PDB from `Tests/obj/IXApi.Tests/Debug/net9.0` to `Tests/bin/FullExampleReview`. An initial copy from the legacy `Tests/obj/Debug/net9.0` directory selected no cases; that run is retained as `cycle-empty-selection.trx` and is not counted as a pass. The final `cycle-final.trx` records exactly five executed tests.
### Broad regression completion and standalone evidence

The broad run completed in 28m33s with **1,127 passed, 2 failed, 21 skipped (1,150 total)**. Both failures were the two reference-cycle tests with the same missing-tax-group error; the binary for that run was compiled before the zero-tax setup repair. Both tests subsequently passed in `cycle-final.trx`, together with the three return-costing regressions. **The original broad run remains failed in its TRX; there was no second all-tests run after the repair.** All changed fixture paths have passing focused rerun evidence; the 1,127 other cases passed their broad execution.

The 21 skips comprise SQL Server integration and cross-connection locking/concurrency checks. Their outcomes remain **Not Run**, not Passed. No SQL Server production-schema or concurrency coverage is claimed. The exact skipped case identities are retained in the broad TRX.

The following standalone examples were re-executed as part of the inventory or broad suite; they are existing tests, not new implementations. The tax examples exercise the configured 15% calculation with their own amounts; they are not represented as newly executed literal 100/15/115 documents.

| Plan category | Executed test evidence | Status |
|---|---|---|
| Complete PO/SO processing | `Standalone_weighted_average_close_moves_the_issue_from_two_hundred_to_two_hundred_fifty` (complete receipt/invoice and complete sale), final focused TRX | Passed |
| Partial PO/SO cancellation | `Cancelling_a_partly_received_order_releases_the_remainder_and_keeps_the_receipt_invoiceable`; `Cancelling_a_partly_delivered_order_releases_the_remainder_and_keeps_the_delivery_invoiceable`, inventory TRX | Passed |
| Quantity matching refusal and correction | Integrated 61-vs-60 rejection, no writes, same-reference corrected retry, final focused TRX; pending-invoice validation cases in broad TRX | Passed |
| Reserve/release and over-reservation | `Reserve_and_release_track_ownership_and_availability`; `Reserve_rejects_more_than_available_physical_stock`, inventory TRX | Passed |
| Inventory journal lifecycle | `Inventory_journal_lifecycle_uses_standard_tables_and_enforces_company_dimensions_and_type_rules`, broad TRX (transfer, adjustment, movement and counting paths) | Passed |
| Weighted average 200→250 and remaining value750 | Standalone reference test, final focused TRX | Passed |
| Return costing and cancellation symmetry | Both `Weighted_average_close_settles_same_period_sale_before_costing_its_return_and_reverses_both` cases and recalculation refusal, final focused TRX | Passed |
| AP/AR settlement reversal | `The_existing_reversal_round_trips_a_settlement_this_service_wrote`; `The_existing_vendor_reversal_round_trips_a_settlement_this_service_wrote`, broad TRX | Passed |
| Payment/collection reversal | `Reversing_a_payment_reopens_the_invoice_it_had_settled`; `Reversing_a_vendor_payment_reopens_the_invoice_it_had_settled`, broad TRX | Passed |
| GL reversal | `Reversal_posts_a_mirrored_journal_linked_to_the_original`, broad TRX | Passed |
| Purchase/sales tax | `Purchase_order_partial_receipts_and_invoices_complete_the_cycle(taxable: True)` and `Free_text_invoice_posts_services_tax_and_receivable_without_inventory`, inventory TRX | Passed |

**Completion boundary:** The implemented service-level integrated reference cycle passes with the stated balances, and the reproduced fixture defects are fixed. This is not a claim that every literal standalone input, all reference fields and dimensions, master-data/configuration CRUD, SQL Server, API routes, or frontend UAT have been verified. Application-service source and application data remain unchanged.
## SQL-enabled follow-up — resolving the 2 failures and 21 skips

The two fixture failures were rerun with SQL locking checks after compiling the corrected source: **9 passed, 0 failed, 0 skipped**, recorded in `TestResults/FullExampleEnabled/fixture-and-locks.trx`.

All **21 formerly skipped cases now have individually matched passing evidence**. The six batch/posting lock and number-sequence concurrency cases are in the 9-case run above; the remaining **15 passed, 0 failed, 0 skipped** are in `TestResults/FullExampleEnabled/formerly-skipped-sql.trx`. A comparison against every `NotExecuted` identity in the earlier broad TRX confirmed exactly one passing result for each. Local SQL Server was verified available before enabling the opt-ins. A read-only check after the focused SQL runs found no remaining disposable databases created by those runs.

Test infrastructure changes:

- `BatchDatabaseLockTests` no longer requires the application `ERM` database. Its session-only lock checks use `master` with GUID-qualified resource names; no application schema or rows are accessed.
- Batch locks and the all-module SQL seeding fixture honor `IXAPI_AP_PHASE_ONE_SQLSERVER`, including the seeding fixture's fault-injection context. Business-cycle SQL tests continue to create uniquely named disposable databases and clean up their own databases.
- `Tests/Run-FullExampleValidation.ps1` enables `IXAPI_AP_PHASE_ONE_SQLSERVER`, `IAX_BATCH_SQL_TESTS=1`, and `IAX_SEED_ERP_V2_SQL_TESTS=1` for both suites, restores the previous process settings afterward, and fails for any non-passing case, missing result, or empty test selection. Default opt-in guards are preserved for machines without SQL Server.

Full validation entry point from the repository root:

```powershell
./IXApi/Tests/Run-FullExampleValidation.ps1 -SqlServer .
```

For this run the final test assembly is compiled with `dotnet build IXApi/Tests/IXApi.Tests.csproj --no-restore -p:BuildProjectReferences=false -p:UseSharedCompilation=false`, followed by the runner with `-NoBuild -NoRestore`. Application-service source is unchanged; existing dependency assemblies are reused.

**Single full SQL-enabled validation invocation:** main suite **1,150 passed, 0 failed, 0 skipped**, recorded in `TestResults/FullExampleEnabled/20261115-145803/IXApi.Tests.trx` (55 minutes). Each of the original 2 failures and 21 skips was matched by test identity to a passing result in this same full run. The inventory suite also passed **147 passed, 0 failed, 0 skipped**, recorded in `TestResults/FullExampleEnabled/20261115-145803/InventoryCycle.Tests.trx` (2m22s). The validation runner exited 0 after independently rejecting any non-passing case in either result file. The earlier failed broad run remains preserved as historical evidence.
### Final single-invocation results

| Suite | Passed | Failed | Skipped | Evidence |
|---|---:|---:|---:|---|
| Main suite, including every SQL opt-in | 1,150 | 0 | 0 | `TestResults/FullExampleEnabled/20261115-145803/IXApi.Tests.trx` |
| Inventory suite | 147 | 0 | 0 | `TestResults/FullExampleEnabled/20261115-145803/InventoryCycle.Tests.trx` |

The final main TRX was checked independently for all 1,150 passing cases, and each of the earlier 23 failing/skipped case identities was matched to exactly one passing result in this same run. No tests were removed, ignored, or converted into artificial passes. SQL cases remain explicitly opt-in in ordinary developer test runs; use the full-validation runner to enable them and require zero skips. No application-service source or application data was changed.

The final read-only SQL Server cleanup check found no remaining disposable test databases created during this follow-up. `git diff --check` also passed.
