# Expected results

## Numerical and behavioral assertions

Prepayment invoice: supplier advance 181005 debit 1,000, vendor 281001 credit 1,000; paid from the bank. Final invoice 2,500 (no sales tax). Application moves the 1,000 from 181005 to the vendor. Net: inventory 2,500; bank -2,500 (1,000 + 1,500); supplier advance 0; vendor 0; nothing open.

## Input basis

Order 100 at 25 with a 1,000 prepayment, tax-free vendor APV2-{company}-PREPAY.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
