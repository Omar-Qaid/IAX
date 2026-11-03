# Expected results

## Numerical and behavioral assertions

Immediately after undo: invoice open 11,500, payment credit open 11,500, net AP 0; bank unchanged. Reapplication closes both.

## Input basis

Invoice 11,500 and payment 11,500 already settled; no discount/FX.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
