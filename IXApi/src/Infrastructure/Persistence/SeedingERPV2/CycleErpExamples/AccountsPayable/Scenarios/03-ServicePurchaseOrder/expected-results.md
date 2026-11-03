# Expected results

## Numerical and behavioral assertions

Expense 1,000; input tax 150; bank -1,150; no stocked on-hand movement. Service receipt accrual depends on configuration.

## Input basis

Service net 1,000; tax 150.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
