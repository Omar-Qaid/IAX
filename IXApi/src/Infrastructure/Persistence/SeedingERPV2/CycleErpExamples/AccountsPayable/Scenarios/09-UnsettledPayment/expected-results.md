# Expected results

## Numerical and behavioral assertions

Before application invoice open 11,500 and payment credit open 6,000; net liability 5,500. After application invoice remaining 5,500 and payment remaining 0.

## Input basis

Invoice 11,500; payment 6,000 without settlement.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
