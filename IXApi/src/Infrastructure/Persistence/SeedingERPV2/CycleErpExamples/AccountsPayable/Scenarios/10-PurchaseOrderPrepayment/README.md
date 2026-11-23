# Purchase Order Prepayment

> Implemented scenario `10`, executed by `AccountsPayableTransactionFlow` with the tax-free vendor `APV2-{company}-PREPAY` (the prepayment invoice path refuses a purchase with a sales tax group). Setup comes from `VendPrepaymentSetupSeeder`: advance posting profile `APV2-ADV` on `VendParameters` (summary 281001), procurement category `APV2 purchase prepayment` with `PurchAdvance` posting to supplier advances 181005. Steps: order 100 at 25; define a 1,000 prepayment (`PurchPrepayTable`); post prepayment invoice `APV2-{year}-10-PRE`; pay it; receive and invoice 2,500 (`-I1`); apply the 1,000 to the final invoice; pay the 1,500 remainder.

> Not implemented. `AccountsPayableTransactionFlow.Scenarios` has no ID 10; this page is a specification only.

## Purpose and inputs

Isolated no-tax PO 10,000; prepayment invoice 2,000.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Create PO; create/post supported prepayment invoice; pay/settle it; receive/final-invoice PO; apply prepayment; pay remaining 8,000.

## Expected result

Advance clears against final invoice; bank total 10,000; final AP 0. Capture actual prepayment interim entries; no double expense/inventory. Add taxable case only with verified tax setup.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.10.PurchaseOrderPrepayment` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
