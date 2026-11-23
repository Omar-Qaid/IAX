# Financial Credit Note

Implemented scenario `15`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

Service invoice net 1,000/tax 150; financial credit net 100/tax 15.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Post original invoice; post linked financial credit; settle credit against invoice.

## Expected result

Remaining AP 1,035; expense reduced 100 and recoverable tax reduced 15; no stock movement.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.15`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
