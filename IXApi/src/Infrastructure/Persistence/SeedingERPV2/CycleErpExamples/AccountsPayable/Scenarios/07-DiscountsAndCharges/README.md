# Discounts And Charges

## Purpose and inputs

100 × 100; discount 10%; capitalized taxable charge 200; tax 15%.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Create priced PO; apply discount and charge through services; receive and invoice; inspect allocations.

## Expected result

Net goods 9,000; capitalized cost 9,200; tax 1,380; payable 10,580. Verify receipt reversal uses receipt valuation and any supported invoice adjustment.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.07.DiscountsAndCharges` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
