# Expected results

## Numerical and behavioral assertions

Not implemented: the seeded examples have no purchase-order prepayment (AccountsPayableTransactionFlow defines no scenario 10). Payment-journal prepayment is scenario 11.

## Input basis

Not implemented.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
