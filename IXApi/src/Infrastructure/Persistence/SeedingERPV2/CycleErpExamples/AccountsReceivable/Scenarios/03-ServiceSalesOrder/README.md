# Service Sales Order

Implemented scenario `03`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

Two maintenance visits at 500; output tax 150; total 1,150. Non-stocked item.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The purchase scenarios for the same fiscal year must have received and financially updated the demo item first, otherwise there is nothing to deliver. Resolve company, customer, items, accounts, tax setup and journal names from the prepared masters.

## Procedure

Create and confirm the order; deliver; invoice; collect 1,150 with settlement.

## Expected result

Issued 0 at every step; revenue 1,000; output tax 150; no inventory or cost entry; bank +1,150; receivable 0.

## Verification and evidence

All eleven checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows once the scenario finishes. Compare quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document and voucher ID. Check that every voucher balances.

## Rerun

The scenario key is `ERPV2.AR.{year}.03`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
