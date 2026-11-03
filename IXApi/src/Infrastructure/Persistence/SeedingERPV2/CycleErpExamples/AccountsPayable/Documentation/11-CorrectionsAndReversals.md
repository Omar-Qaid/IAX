# Corrections and reversals

| Operation | Preconditions | Required result |
|---|---|---|
| Cancel unreceived balance | Open order with eligible remaining quantity | Reduce remaining obligation, retain posted history |
| Correct receipt | Receipt quantity eligible under business rules | Linked physical reversal and accrual correction |
| Return invoiced stock | Eligible stock and original-cost/source link | Stock issue plus vendor credit through supported return flow |
| Financial credit | Correctable invoice/expense | Vendor/tax/expense adjustment without physical movement |
| Reverse settlement | Reversible allocations and supported dates | Reopened amounts and linked adjustment reversals |
| Reverse payment | Eligible payment status and bank process | Opposite bank/vendor effect and correct settlement treatment |

## Traceability

Retain original document, correcting document, voucher, date, reason, user/process reference and original settlement links. Do not delete posted entries to reset a scenario. Never force status fields to bypass supported transition rules.

## Dates and closed periods

Respect period locks and supported correction dates. A scenario must report the exact blocked operation when original-date reversal is disallowed. Use a supported current-period correction only when explicitly modeled and permitted.

## Rerun

If a reversal already exists, resolve its durable scenario reference and validate it. Do not reverse it a second time. Settlement reversal requires recomputing the affected open balances; it is not equivalent to deleting one allocation row.
