# Expected results

## Numerical and behavioral assertions

Inventory 2,500; delivery expense 50; input tax 382.50 (15% of 2,550); bank -2,932.50.

## Input basis

100 units at 25 plus a 50 delivery charge.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
