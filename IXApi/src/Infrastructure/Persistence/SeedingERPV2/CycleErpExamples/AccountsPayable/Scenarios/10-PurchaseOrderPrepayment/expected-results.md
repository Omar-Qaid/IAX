# Expected results

## Numerical and behavioral assertions

Advance clears against final invoice; bank total 10,000; final AP 0. Capture actual prepayment interim entries; no double expense/inventory. Add taxable case only with verified tax setup.

## Input basis

Isolated no-tax PO 10,000; prepayment invoice 2,000.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
