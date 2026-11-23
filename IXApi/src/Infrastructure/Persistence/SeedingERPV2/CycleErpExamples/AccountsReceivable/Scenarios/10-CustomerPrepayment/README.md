# Customer Prepayment

Implemented scenario `10`, executed by `AccountsReceivableTransactionFlow` through the services that own each document.

## Purpose and inputs

A 1,000 customer prepayment received before invoicing, applied to the later 4,600 invoice.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The AR setup seeds the prepayment posting profile `ARV2-PRE`, whose summary account is the customer-advance liability, and `CustParameters.PrepaymentPostingProfile` names it.

## Procedure

Confirm the order; post a 1,000 prepayment through the customer payment service with `Prepayment = true` and no invoice; deliver and invoice 100; settle the prepayment against the invoice; collect the remaining 3,600 against the invoice.

## Expected result

Issued -100; the advance account nets to zero once the prepayment is applied; both open amounts are 0; bank +4,600 in two receipts.

## Verification and evidence

All checks in [Reconciliation](../../Documentation/12-Reconciliation.md) run against the persisted rows, including the check that an applied customer prepayment leaves nothing on the advance account. Compare with [Expected results](expected-results.md).

## Rerun

Every step resolves the document it would post before posting it, so a rerun verifies and resumes without reposting.

See [Impacted tables](impacted-tables.md) and [Accounting entries](accounting-entries.md).
