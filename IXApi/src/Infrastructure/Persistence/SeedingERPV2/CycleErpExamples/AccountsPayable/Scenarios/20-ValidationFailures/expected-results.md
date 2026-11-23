# Expected results

## Numerical and behavioral assertions

Rejected operations post nothing; the valid documents stand: physical 60; inventory 1,500; input tax 225; bank -1,725.

## Input basis

Same as 16, with invalid operations attempted.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
