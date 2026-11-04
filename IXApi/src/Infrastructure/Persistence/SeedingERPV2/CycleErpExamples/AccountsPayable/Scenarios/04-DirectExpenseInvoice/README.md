# Direct Expense Invoice

> Future transaction specification only. Current CycleErpExamples prepares configuration/master data and does not execute these steps.

## Purpose and inputs

Expense net 1,000; tax 150; total 1,150.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Create invoice journal; validate account/tax; post; pay and settle.

## Expected result

Expense debit 1,000; input tax debit 150; bank credit 1,150; AP closed; no PO receipt or stocked movement.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.04.DirectExpenseInvoice` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
