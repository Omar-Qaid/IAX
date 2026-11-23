# Expected results

## Numerical and behavioral assertions

Bank -1,000; vendor control -1,875; the invoice stays open for 1,875.

## Input basis

Invoice 2,875 (100 at 25 plus tax); payment 1,000.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
