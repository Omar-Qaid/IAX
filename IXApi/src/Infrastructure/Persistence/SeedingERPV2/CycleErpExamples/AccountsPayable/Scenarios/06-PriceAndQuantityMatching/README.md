# Price And Quantity Matching

Implemented scenario `06`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

Order 100 × 100; receive 60; strict zero-tolerance matching for isolated setup.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Attempt invoice for 61 received units; reject. Attempt price 101; reject under strict policy. Correct to 60 × 100; post.

## Expected result

Rejected attempts create no posted vendor/tax/GL effect; accepted invoice gross 6,900. Verify configured exceptions cannot bypass required approval.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

The scenario key is `ERPV2.AP.{year}.06`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
