# Expected results

## Numerical and behavioral assertions

Physical 100; inventory 2,500; input tax 375; bank -2,875; no receipt quantity invoiced twice.

## Input basis

100 units at 25 received in two receipts and invoiced against both.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
