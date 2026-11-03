# Invoice Across Multiple Receipts

## Purpose and inputs

Order 100 × 100; receipts 30 and 70.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Post both receipts; select 30 from first and 20 from second for invoice 50; invoice remaining 50 later.

## Expected result

First invoice gross 5,750; remaining uninvoiced receipt net 5,000; second invoice gross 5,750; no receipt quantity invoiced twice.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.05.InvoiceAcrossMultipleReceipts` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
