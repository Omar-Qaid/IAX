# Payment Journal Prepayment

Implemented scenario `11`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

No-tax final expense invoice 10,000; advance payment 2,000.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Post supported vendor advance journal; verify advance account; post final invoice; apply advance; pay remaining 8,000.

## Expected result

Advance asset initially debit 2,000; application credits advance 2,000/debits AP 2,000; final AP 0 and bank credit total 10,000.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.11`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
