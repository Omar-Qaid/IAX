# Unsettled Payment

Implemented scenario `09`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

Invoice 11,500; payment 6,000 without settlement.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Post invoice and payment with auto-settlement disabled; validate both open; explicitly settle later.

## Expected result

Before application invoice open 11,500 and payment credit open 6,000; net liability 5,500. After application invoice remaining 5,500 and payment remaining 0.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.09`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
