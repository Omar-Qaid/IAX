# Expected results

## Numerical and behavioral assertions

Physical +100; inventory 10,000; input tax 1,500; bank -11,500; payable/open/accrual 0.

## Input basis

100 units × 100; tax 1,500; total 11,500.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
