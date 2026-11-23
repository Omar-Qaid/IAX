# Expected results

## Numerical and behavioral assertions

Invoice reopens: vendor control -2,875, open 2,875; bank original and reversal net 0.

## Input basis

Invoice 2,875 paid, then the payment reversed.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
