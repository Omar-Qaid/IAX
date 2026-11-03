# Validation Failures

## Purpose and inputs

Separate attempts: duplicate vendor invoice, blocked vendor, closed period, missing posting account, overreceipt/overinvoice.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). Resolve company, vendor, applicable products, accounts, tax setup and journal names. Start from scenario-specific documents and captured opening balances. Do not reuse another scenario's posted invoice unless the dependency is explicit.

## Procedure

Set explicit isolated failure preconditions; call business service; capture expected error; restore test setup through safe supported actions.

## Expected result

No posted partial vendor/inventory/tax/bank/GL effects for rejected operation. Valid prior documents remain unchanged. A permissive configured duplicate policy must be reported rather than assumed strict.

## Verification and evidence

Compare actual quantities, document totals, open amounts, settlements, inventory values and vouchers with [Expected results](expected-results.md). Capture every generated document ID and voucher ID. Check every voucher balances. Validate rejected operations at their transaction boundary. Use exact currency precision and record any rounding tolerance.

## Rerun

Use tenant/company + stable scenario key `ap.demo.20.ValidationFailures` and step-specific operation keys. Resolve an existing posted document before calling posting again. Validate it and resume incomplete steps. Do not recreate, delete or repost completed history.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
