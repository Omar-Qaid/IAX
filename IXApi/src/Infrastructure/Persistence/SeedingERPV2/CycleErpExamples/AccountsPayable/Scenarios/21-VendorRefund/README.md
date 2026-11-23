# Vendor Refund

Implemented scenario `21`, executed by `AccountsPayableTransactionFlow` through the services that own each document.

## Purpose and inputs

A financial credit note of SAR 115 (no goods returned) that the vendor repays into the bank.

## Prerequisites

Run the dependency graph in [Dependencies](../../Documentation/02-Dependencies.md). The vendor account is `APV2-{company}-LOCAL` and the bank account is `APV2-BANK`.

## Procedure

1. Post the credit note `APV2-{year}-21-CR` (115) through the AP invoice journal.
2. Recover it with `IVendPaymentService.RefundAsync` as document `APV2-{year}-21-REF`, dated three days later. The refund settles the credit note against the bank receipt.

## Expected result

No inventory movement. Bank +115. The credit note and the refund are settled, so the vendor's open balance is 0.

## Rerun

The scenario key is `ERPV2.AP.{year}.21`, and every step resolves the document it would post before posting it. A rerun verifies and resumes; it does not recreate, delete or repost completed history.
