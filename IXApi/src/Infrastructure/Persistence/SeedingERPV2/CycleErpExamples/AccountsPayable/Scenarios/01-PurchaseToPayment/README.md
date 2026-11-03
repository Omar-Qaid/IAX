# Purchase To Payment

## Purpose and inputs

100 units × 100; tax 1,500; total 11,500.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Create and confirm PO; receive 100; invoice 100; pay/apply 6,000; pay/apply 5,500.

## Expected result

Physical +100; inventory 10,000; input tax 1,500; bank -11,500; payable/open/accrual 0.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.01.PurchaseToPayment` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
