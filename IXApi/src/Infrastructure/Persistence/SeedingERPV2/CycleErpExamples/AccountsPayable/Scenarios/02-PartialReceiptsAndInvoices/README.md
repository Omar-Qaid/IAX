# Partial Receipts And Invoices

## Purpose and inputs

100 units × 100; illustrative tax 15%.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Receive 60; invoice 40 (4,600 gross); receive 40; invoice 60 (6,900 gross).

## Expected result

Physical 100; financial quantity 100; gross payable 11,500 before payment; accrual 0. At first invoice uninvoiced receipt is 20 units/2,000.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.02.PartialReceiptsAndInvoices` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
