# Expected results

## Numerical and behavioral assertions

Net physically received 50; receipt asset/accrual 5,000 if enabled; no vendor invoice balance; rejected attempt has no extra effect.

## Input basis

Order 100 × 100; receive 60; none invoiced.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
