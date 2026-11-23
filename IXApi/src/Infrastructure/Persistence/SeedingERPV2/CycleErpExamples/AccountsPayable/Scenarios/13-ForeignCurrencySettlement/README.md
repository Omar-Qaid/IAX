# Foreign Currency Settlement

Implemented scenario `13`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

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

The scenario key is `ERPV2.AP.{year}.13`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
