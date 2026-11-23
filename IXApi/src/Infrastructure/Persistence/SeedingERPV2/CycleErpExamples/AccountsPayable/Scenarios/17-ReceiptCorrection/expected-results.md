# Expected results

## Numerical and behavioral assertions

Physical 50 on hand; the open receipt accrual is 1,250 (50 x 25): received-not-invoiced interim 181003 debit 1,250 and accrual 281002 credit 1,250. No vendor transaction, tax or bank movement. The rejected correction to 110 posts nothing.

## Input basis

Order 100 at 25; receive 100; correct to 50.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
