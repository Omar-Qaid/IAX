# Foreign Currency Sale

Implemented scenario `13`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

100 reams at USD 11 to the USD customer; invoice rate 3.75, collection rate 3.80.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The purchase scenarios for the same fiscal year must have received and financially updated the demo item first, otherwise there is nothing to deliver. Resolve company, customer, items, accounts, tax setup and journal names from the prepared masters.

## Procedure

Deliver and invoice 100 in USD; collect USD 1,100 into the USD bank account with settlement.

## Expected result

Issued -100; receivable 0; a realized exchange difference is recognized for the rate movement.

## Verification and evidence

All eleven checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows once the scenario finishes. Compare quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document and voucher ID. Check that every voucher balances.

## Rerun

The scenario key is `ERPV2.AR.{year}.13`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
