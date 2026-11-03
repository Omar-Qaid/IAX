# Foreign Currency Settlement

## Purpose and inputs

No-tax invoice USD 1,000; invoice rate 3.75 and payment rate 3.80 SAR/USD.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Configure rate direction/factors and effective dates; post invoice; post USD payment; settle.

## Expected result

Historical AP 3,750 SAR; bank 3,800 SAR; realized loss 50 SAR; USD balance 0. Aggregate across settlement/payment vouchers.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.13.ForeignCurrencySettlement` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
