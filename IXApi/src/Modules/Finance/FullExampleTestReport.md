# Core ERP cycle — QA execution report (PlanTestForFullEmaple.md)

**Run date:** 2026-11-15 · **Branch:** `feat/ap-pending-vendor-invoice` · **Database:** isolated in-memory SQLite per test (never the hosted database) · **Entry points:** the real Finance services, invoked by `IXApi/Tests/FullExampleCycleTests.cs` and `IXApi/Tests/InventoryWeightedAverageReturnCloseTests.cs` · **Suite:** `dotnet test Tests/IXApi.Tests.csproj` — 1128 passed, 0 failed, 21 skipped.

The integrated cycle below is executed by one ordered test,
`Core_erp_reference_cycle_closes_at_fifty_six_pieces_and_a_profit_of_nineteen_hundred`, whose
asserts are the database evidence: every checkpoint reads `InventTrans`, `InventSum`, `PurchLine`,
`SalesLine`, `VendTrans/Open`, `CustTrans/Open`, `BankAccountTrans`, `InventSettlement` and
`GeneralJournalAccountEntry` directly, never an API status alone.

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
