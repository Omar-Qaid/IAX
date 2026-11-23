# Expected results

## Numerical and behavioral assertions

Both rejected attempts post nothing. The accepted invoice: inventory 2,500; input tax 375; bank -2,875.

## Input basis

100 units at 25; an invoice at the wrong price and one for excess quantity are attempted first.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
