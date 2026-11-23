# Expected results

## Numerical and behavioral assertions

Inventory 2,625 SAR; input tax 393.75 SAR; bank -805 USD = -3,059 SAR; realized exchange loss 40.25 SAR; USD vendor balance 0.

## Input basis

100 units at 7 USD (700 USD) plus 105 USD tax at 3.75; paid 805 USD at 3.80.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
