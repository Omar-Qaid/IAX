# Purchase To Payment

Implemented scenario `01`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

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

The scenario key is `ERPV2.AP.{year}.01`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
