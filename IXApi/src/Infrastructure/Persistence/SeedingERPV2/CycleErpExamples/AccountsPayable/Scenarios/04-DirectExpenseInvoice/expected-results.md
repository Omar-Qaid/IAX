# Expected results

## Numerical and behavioral assertions

Expense debit 1,000; input tax debit 150; bank credit 1,150; AP closed; no PO receipt or stocked movement.

## Input basis

Expense net 1,000; tax 150; total 1,150.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
