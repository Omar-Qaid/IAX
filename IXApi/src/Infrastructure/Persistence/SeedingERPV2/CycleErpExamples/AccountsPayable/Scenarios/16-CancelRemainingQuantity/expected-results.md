# Expected results

## Numerical and behavioral assertions

Received/invoiced remain 60; receipt obligation 0; cancelled quantity 40; invoice total 6,900; no reversal of posted 60.

## Input basis

Order 100 × 100; receive and invoice 60.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
