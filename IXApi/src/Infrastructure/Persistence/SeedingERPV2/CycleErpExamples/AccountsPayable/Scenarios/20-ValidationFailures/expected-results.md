# Expected results

## Numerical and behavioral assertions

No posted partial vendor/inventory/tax/bank/GL effects for rejected operation. Valid prior documents remain unchanged. A permissive configured duplicate policy must be reported rather than assumed strict.

## Input basis

Separate attempts: duplicate vendor invoice, blocked vendor, closed period, missing posting account, overreceipt/overinvoice.

## Acceptance record

Record assertion name, company, document/line, expected value, actual value, unit/currency, precision, status and related voucher. Query actual persisted data after the business operation commits. Expected amounts must be independently calculated from inputs, not copied from the posting service response.

Reconcile to scenario opening balances and confirm no cross-company references. Report blocked assertions explicitly. Repeating the same scope must preserve document counts and posted amounts.
