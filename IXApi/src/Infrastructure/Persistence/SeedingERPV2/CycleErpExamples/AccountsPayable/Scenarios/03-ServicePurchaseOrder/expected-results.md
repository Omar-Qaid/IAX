# Expected results

## Numerical and behavioral assertions

Expense 700; input tax 105; bank -805; no stocked on-hand movement.

## Input basis

Service purchase order, 2 units at 350.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
