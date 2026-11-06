# On Account Collection

Implemented scenario `09`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

A 4,600 invoice and a collection posted on account before being settled.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The purchase scenarios for the same fiscal year must have received and financially updated the demo item first, otherwise there is nothing to deliver. Resolve company, customer, items, accounts, tax setup and journal names from the prepared masters.

## Procedure

Deliver and invoice 100; collect 4,600 without naming an invoice; settle the marked transactions separately.

## Expected result

Issued -100; after settlement both open amounts are 0; bank +4,600; CustSettlement names both sides.

## Verification and evidence

All eleven checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows once the scenario finishes. Compare quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document and voucher ID. Check that every voucher balances.

## Rerun

The scenario key is `ERPV2.AR.{year}.09`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
