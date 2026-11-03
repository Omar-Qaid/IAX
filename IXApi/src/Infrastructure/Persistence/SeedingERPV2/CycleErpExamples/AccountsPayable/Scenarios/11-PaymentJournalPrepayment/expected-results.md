# Expected results

## Numerical and behavioral assertions

Advance asset initially debit 2,000; application credits advance 2,000/debits AP 2,000; final AP 0 and bank credit total 10,000.

## Input basis

No-tax final expense invoice 10,000; advance payment 2,000.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
