# Service Purchase Order

Implemented scenario `03`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

Service net 1,000; tax 150.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Create service PO using supported nonstocked/category line; confirm; record service receipt if supported; invoice; pay and settle 1,150.

## Expected result

Expense 1,000; input tax 150; bank -1,150; no stocked on-hand movement. Service receipt accrual depends on configuration.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.03`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
