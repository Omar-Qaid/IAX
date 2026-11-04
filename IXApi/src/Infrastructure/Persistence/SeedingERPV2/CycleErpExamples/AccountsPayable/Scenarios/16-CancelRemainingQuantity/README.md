# Cancel Remaining Quantity

> Future transaction specification only. Current CycleErpExamples prepares configuration/master data and does not execute these steps.

## Purpose and inputs

Order 100 × 100; receive and invoice 60.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Cancel eligible unreceived remaining 40 through purchasing operation.

## Expected result

Received/invoiced remain 60; receipt obligation 0; cancelled quantity 40; invoice total 6,900; no reversal of posted 60.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.16.CancelRemainingQuantity` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
