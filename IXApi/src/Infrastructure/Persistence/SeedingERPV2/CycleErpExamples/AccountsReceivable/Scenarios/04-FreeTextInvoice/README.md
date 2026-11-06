# Free Text Invoice

Implemented scenario `04`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

One advisory line at 350; output tax 52.50; total 402.50. No sales order and no item.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The purchase scenarios for the same fiscal year must have received and financially updated the demo item first, otherwise there is nothing to deliver. Resolve company, customer, items, accounts, tax setup and journal names from the prepared masters.

## Procedure

Create the free text invoice with its revenue account on the line; post it; collect 402.50 against it.

## Expected result

Issued 0; revenue 350; output tax 52.50; bank +402.50; receivable 0; no InventTrans row exists for this scenario.

## Verification and evidence

All eleven checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows once the scenario finishes. Compare quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document and voucher ID. Check that every voucher balances.

## Rerun

The scenario key is `ERPV2.AR.{year}.04`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
