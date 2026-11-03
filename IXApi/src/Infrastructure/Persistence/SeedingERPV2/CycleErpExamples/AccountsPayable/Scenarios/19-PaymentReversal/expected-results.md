# Expected results

## Numerical and behavioral assertions

Invoice reopens 11,500; original and reversal payment net 0; bank original/reversal net 0. Verify no unintended unapplied credit remains.

## Input basis

Invoice 11,500 settled by eligible bank payment 11,500.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
