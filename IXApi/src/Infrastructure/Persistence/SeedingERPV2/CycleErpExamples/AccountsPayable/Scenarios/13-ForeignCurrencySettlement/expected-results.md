# Expected results

## Numerical and behavioral assertions

Historical AP 3,750 SAR; bank 3,800 SAR; realized loss 50 SAR; USD balance 0. Aggregate across settlement/payment vouchers.

## Input basis

No-tax invoice USD 1,000; invoice rate 3.75 and payment rate 3.80 SAR/USD.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
