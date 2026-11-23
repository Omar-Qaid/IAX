# Partial Receipts And Invoices

Implemented scenario `02`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

100 units × 100; illustrative tax 15%.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Receive 60; invoice 40 (4,600 gross); receive 40; invoice 60 (6,900 gross).

## Expected result

Physical 100; financial quantity 100; gross payable 11,500 before payment; accrual 0. At first invoice uninvoiced receipt is 20 units/2,000.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.02`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
