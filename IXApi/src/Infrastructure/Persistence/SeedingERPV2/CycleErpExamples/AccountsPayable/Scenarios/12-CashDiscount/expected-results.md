# Expected results

## Numerical and behavioral assertions

Eligible: AP closes with bank 9,800 and discount 200. Ineligible: no discount; remaining 200 unless fully paid.

## Input basis

No-tax invoice 10,000; eligible discount 2%; payment 9,800.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
