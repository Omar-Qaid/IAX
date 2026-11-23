# Overpayment

Implemented scenario `11`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

A 4,600 invoice collected with 4,700: the extra 100 stays open as a customer credit.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md).

## Procedure

Deliver and invoice 100; collect 4,700 against the invoice with automatic settlement.

## Expected result

Issued -100; the invoice is closed; the payment keeps an open credit of 100 on the customer; bank +4,700.

## Verification and evidence

All checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows. Compare with [Expected results](expected-results.md).

## Rerun

A rerun verifies and resumes; it does not repost.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
