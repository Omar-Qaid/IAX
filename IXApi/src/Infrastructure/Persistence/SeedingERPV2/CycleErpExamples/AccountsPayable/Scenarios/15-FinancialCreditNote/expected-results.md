# Expected results

## Numerical and behavioral assertions

Net expense 250; input tax 37.50; bank -287.50; no stock movement.

## Input basis

Expense invoice with a financial credit note, no inventory.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
