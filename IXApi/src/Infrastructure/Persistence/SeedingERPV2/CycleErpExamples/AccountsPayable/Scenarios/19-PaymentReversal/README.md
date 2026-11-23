# Payment Reversal

Implemented scenario `19`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

Invoice 11,500 settled by eligible bank payment 11,500.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Use supported undo-settlement/payment-reversal sequence; respect bank/payment status checks.

## Expected result

Invoice reopens 11,500; original and reversal payment net 0; bank original/reversal net 0. Verify no unintended unapplied credit remains.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.19`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
