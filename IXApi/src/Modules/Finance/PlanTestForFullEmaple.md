Act as the QA engineer responsible for executing and verifying the full **Core ERP accounting cycle** in this workspace. Cover **Procurement and Accounts Payable, Sales and Accounts Receivable, Basic Inventory, and General Ledger**. Use the project's documented D365 F&O behavior as the acceptance reference. Keep the work within basic ERP. Exclude WHS, production, manufacturing, and advanced workflows.

## 1. Prepare the test environment

- Read the project instructions, existing acceptance references, seed data, and previous test results. Check active test jobs before starting a new run. If another run already covers a case, check its result before repeating it.
- Identify the normal entry point for each service, the database, and the actual tables and fields. Map D365 reference names to the project's implementation where they differ.
- Run in an isolated test database. Preserve user changes, existing seed data, and historical evidence. Use unique document references for this run.
- Establish a clean baseline. Use one test company in `SAR`, one vendor `V-001`, one customer `C-001`, one stocked item `ITEM-01`, one non-stocked sales service, warehouses `A` and `B`, and a transit warehouse. Use `NET30`, Weighted Average, and zero opening inventory and trade balances. Post an opening bank balance of **10,000 SAR** against capital.
- Use the project's actual application entry points to create and post transactions. Use read-only database queries to inspect results. A passing API response alone does not establish that posting succeeded.

## 2. Execute the integrated cycle in this order

Record **one result for each action**. For example, report Sales Order Create, Sales Order Confirm, Reserve, Packing Slip Post, and Invoice Post as separate cases.

| Phase | Services and actions to execute | Required checks |
|---|---|---|
| **01 — Configuration and master data** | Verify ledger, open period, account structures, posting profiles, financial dimensions, journal names, number sequences, payment terms and methods, vendor, customer, stocked item, service, site and warehouses. | Verify the effective posting account and dimensions from a posted voucher. Confirm the stocked item uses Weighted Average and the service is non-stocked. |
| **02 — Purchase order** | Create and confirm `PO-001` for **100 × 50** into `A`. Post receipt of **60**, match and post the first invoice, then receive and invoice the remaining **40**. | After the first receipt, physical stock is **60** and remaining physical/financial quantities are **40/100**. After its invoice, they are **40/40**, stock remains **60**, and vendor liability is **3,000**. After the final invoice, remaining quantities are **0/0**, stock is **100**, and receipt accrual for the completed PO is **0**. |
| **03 — Purchase services and direct invoices** | Post a purchase service invoice for **200** and a direct vendor expense invoice for **80**. | Confirm the expense accounts and AP open transactions. Inventory quantity and value stay unchanged. |
| **04 — Transfer order** | Create `TR-001` for **30** from `A` to `B`. Post shipment, then receipt. | After shipment, `A=70`, `B=0`, `TRANSIT=30`. After receipt, `A=70`, `B=30`, `TRANSIT=0`. Total inventory value remains **5,000**. |
| **05 — Sales order** | Create and confirm `SO-001` for **40 × 100** from `A`, plus a non-stocked service for **300**. Reserve the stock. Post a packing slip and invoice for **25**, then a second packing slip for **15** and invoice the **15** plus the service. | Reservation reduces available stock while physical stock stays **70**. After the first packing slip, `A=45` and remaining physical/financial quantities are **15/40**. After its invoice, they are **15/15**, stock stays **45**, customer liability is **2,500**, and cost of sales is **1,250**. Final invoice **1,800**, final remaining quantities **0/0**, and `A=30`. |
| **06 — Direct sales invoice and returns** | Post a Free Text Invoice for **100**. Return **5** pieces to the vendor for a **250** credit. Receive **2** back from the customer and post a **200** credit linked to the original sale. Post a separate **50** credit-only adjustment. | Vendor return takes `A` from **30 to 25** and reduces inventory value by **250**. Customer return takes `A` from **25 to 27** and returns **100** of cost to stock. Credit-only reduces AR and net revenue by **50** with zero quantity or cost impact. |
| **07 — AP payment and settlement** | Pay **3,000** against the first vendor invoice. Post a separate **1,000** payment without marking an invoice. Verify it is open, then settle it and the **250** vendor credit against the second invoice. | First invoice closes. Second invoice remains open for **750**. With the two expense invoices, total open AP is **1,030**. |
| **08 — AR collection and settlement** | Collect **2,000** against the first sales invoice. Post a separate **1,000** collection without marking an invoice. Verify it is open, then allocate **500** to each sales invoice. Settle the **200** and **50** credits against the second invoice. | First sales invoice closes. Second invoice remains open for **1,050**, plus the **100** Free Text Invoice. Total open AR is **1,150**. |
| **09 — Counting journal and costing** | In `B`, enter a count of **29** against system stock of **30**, then post. Complete inventory close for the period. | The posted difference is **−1**, with **50** charged to inventory loss. Final stock is `A=27`, `B=29`, `TRANSIT=0`. Inventory valuation is **2,800**. Compare the report with GL and inspect the costing settlement results. |
| **10 — GL and period close** | Post a bank expense of **20**. Reconcile bank, AP, AR, inventory, receipt accrual, and delivered-not-invoiced accounts. Review the trial balance and income statement. Put the test period on hold and attempt another posting. | Every posted voucher balances. Bank is **8,980**. AP is **1,030**. AR is **1,150**. Inventory is **2,800**. Net revenue is **4,150** and profit is **1,900**. Posting to the held period is rejected. |

## 3. Run standalone tests

Use separate documents, items, periods, or an isolated database reset so these cases do not change the integrated cycle's expected balances.

- **Full and partial processing:** Execute one purchase and one sale fully in a single batch. Test cancellation of the remaining open quantity after partial processing. Confirm posted quantities and vouchers remain traceable.
- **Invoice matching:** Attempt to invoice **61** against **60** received with a policy configured to reject the variance. Verify the expected rejection and confirm that no posted invoice, vendor liability, or voucher was created. Correct to **60** and retest.
- **Stock controls:** Reserve and release stock. Attempt to post an issue beyond available stock with negative inventory disabled. Test a direct transfer journal, positive and negative adjustments, and a count with zero difference.
- **Weighted Average:** On an independent item, invoice **10 × 40**, sell and invoice **5**, then invoice **10 × 60** within the same costing period and dimensions, without marking. Verify issue cost changes from **200** to **250** after inventory close. Remaining stock is **15** with a value of **750**.
- **Corrections:** Undo a settlement and settle it again. Reverse a posted payment or collection through the supported entry point. Reverse a test GL journal. Verify reopened balances, reversal vouchers, and bank impact where applicable.
- **Tax:** Post one purchase and one sale with a **100** taxable base and a configured **15%** test rate. Verify **15** tax, **115** total, the tax transaction, the party balance, and the GL account.

## 4. Capture database evidence for every posting step

Use these D365 reference names as a field checklist. In the test report, write the **actual project table and field names** if they differ.

| What to verify | Reference tables and fields |
|---|---|
| **Purchase order lifecycle** | `PurchTable`, `PurchLine`, `VendPurchOrderJour/Trans`, `VendPackingSlipJour/Trans`, `VendInvoiceJour/Trans`. Capture `PurchId`, `PurchQty`, `PurchPrice`, `RemainPurchPhysical`, `RemainPurchFinancial`, receipt reference, invoice reference, and voucher. |
| **Sales order lifecycle** | `SalesTable`, `SalesLine`, `CustConfirmJour/Trans`, `CustPackingSlipJour/Trans`, `CustInvoiceJour/Trans`. Capture `SalesId`, `SalesQty`, `SalesPrice`, `RemainSalesPhysical`, `RemainSalesFinancial`, delivery reference, invoice reference, and voucher. |
| **Inventory quantity and cost** | `InventTransOrigin`, `InventTrans`, `InventDim`, and on-hand inquiry. Capture `InventTransId`, `InventDimId`, `Qty`, `StatusReceipt`, `StatusIssue`, `DatePhysical`, `DateFinancial`, `VoucherPhysical`, `Voucher`, `CostAmountPosted`, and `CostAmountAdjustment`. Verify `Received → Purchased` for purchase receipts and `Deducted → Sold` for sales issues. |
| **Transfer, counting, and close** | `InventTransferTable/Line`, `InventTransferJour/JourLine`, `InventJournalTable/Trans`, `InventSettlement`. Capture `TransferId`, `QtyShipped`, `QtyReceived`, remaining quantities, `JournalId`, `Counted`, `Qty`, and settlement adjustment. |
| **AP, AR, and bank** | `VendTrans/Open`, `CustTrans/Open`, `VendSettlement`, `CustSettlement`, `BankAccountTrans`. Capture original amount, open amount, due date, `RefRecId`, settlement amount, settlement references, and bank voucher. |
| **General ledger** | `LedgerJournalTable/Trans`, `SubledgerVoucherGeneralJournalEntry` where applicable, `GeneralJournalEntry`, `GeneralJournalAccountEntry`. Capture company, posting date, voucher, main account, dimensions, posting type, and debit/credit amounts. |

At each checkpoint, compare the **source document**, **physical quantity**, **inventory cost**, **party balance**, and **GL voucher**. Record evidence before proceeding to the next phase. For partial postings, verify the split quantities against the original line. For corrections and safe retries, verify that the result contains no duplicate posted document, stock movement, party transaction, or voucher.

## 5. Report results and finish the run

Use these columns for each action.

`Test ID | Phase | Service / Entry Point | Input | Expected | Actual | Tables and Fields Verified | Document / Voucher | Status | Evidence`

Use `Passed`, `Failed`, `Blocked`, or `Not Run`. For a repaired case, keep the history as **Failed → Fixed → Passed**. Record the first failure, root cause, fix, and rerun evidence. Fix in-scope defects only if the task authorizes code changes. Rerun the failed case and affected downstream cases after a fix.

Mark the full cycle **Passed** only when every in-scope case has executed successfully and the final balances reconcile to **56 pieces**, **2,800 inventory**, **1,030 AP**, **1,150 AR**, **8,980 bank**, and **1,900 profit**. If any case is blocked or cannot run, report the exact dependency and the affected cases. Include the test database identity, commands or test entry points, evidence references, and final reconciliation in the completion report.