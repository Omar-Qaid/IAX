# Settlement Reversal

> Future transaction specification only. Current CycleErpExamples prepares configuration/master data and does not execute these steps.

## Purpose and inputs

Invoice 11,500 and payment 11,500 already settled; no discount/FX.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Reverse supported settlement; inspect reopened amounts; reapply if scenario includes recovery.

## Expected result

Immediately after undo: invoice open 11,500, payment credit open 11,500, net AP 0; bank unchanged. Reapplication closes both.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.18.SettlementReversal` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
