# Cancel Undelivered Remainder

Implemented scenario `16`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

100 reams ordered, 60 delivered, the remaining 40 no longer wanted.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The purchase scenarios for the same fiscal year must have received and financially updated the demo item first, otherwise there is nothing to deliver. Resolve company, customer, items, accounts, tax setup and journal names from the prepared masters.

## Procedure

Deliver 60; cancel the order remainder; invoice the delivered 60; collect 2,760 with settlement.

## Expected result

Issued -60; revenue 2,400; output tax 360; the remaining 40 is closed and never delivered; receivable 0.

## Verification and evidence

All eleven checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows once the scenario finishes. Compare quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document and voucher ID. Check that every voucher balances.

## Rerun

The scenario key is `ERPV2.AR.{year}.16`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
