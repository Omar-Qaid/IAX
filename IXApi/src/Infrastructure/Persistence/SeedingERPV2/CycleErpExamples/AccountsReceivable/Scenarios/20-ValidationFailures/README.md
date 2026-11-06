# Validation Failures

Implemented scenario `20`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

An invoice for more than was delivered must be refused atomically.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The purchase scenarios for the same fiscal year must have received and financially updated the demo item first, otherwise there is nothing to deliver. Resolve company, customer, items, accounts, tax setup and journal names from the prepared masters.

## Procedure

Deliver 60 of 100; attempt to invoice 101; verify the refusal wrote nothing; then invoice the delivered 60 and collect.

## Expected result

The excess attempt is rejected with 400 or 422; every posting table is unchanged by it; the valid invoice then posts normally.

## Verification and evidence

All eleven checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows once the scenario finishes. Compare quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document and voucher ID. Check that every voucher balances.

## Rerun

The scenario key is `ERPV2.AR.{year}.20`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
