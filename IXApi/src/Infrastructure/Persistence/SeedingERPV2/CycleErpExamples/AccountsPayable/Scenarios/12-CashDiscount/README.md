# Cash Discount

Implemented scenario `12`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

No-tax invoice 10,000; eligible discount 2%; payment 9,800.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Configure discount dates; post invoice; pay within eligible period and settle with discount. Also test payment outside eligibility.

## Expected result

Eligible: AP closes with bank 9,800 and discount 200. Ineligible: no discount; remaining 200 unless fully paid.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.12`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
