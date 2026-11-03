# Reconciliation and acceptance criteria

## Required validators

| Validator | Assertion |
|---|---|
| Document totals | Net + allocated charges + tax = gross within configured precision |
| Quantity | Received/invoiced/cancelled/returned balances follow supported line rules |
| Physical stock | Scenario movements equal dimension-scoped on-hand change |
| Financial inventory | Posted cost agrees with inventory ledger and costing adjustments |
| Receipt accrual | Remaining accrual agrees with eligible received-not-invoiced value |
| Vendor balance | Signed posted transactions agree with remaining signed open amounts |
| Settlement | Applications and adjustments explain remaining balances |
| AP versus GL | Vendor subledger agrees with applicable control-account movement |
| GL voucher | Debits equal credits in accounting currency |
| Tax | Tax document lines agree with tax transaction and tax-account movement |
| Bank | Posted payment agrees with bank transaction and ledger movement |
| Traceability | Documents link to movements, settlements and vouchers |
| Rerun | Same run scope creates no duplicate posting or master records |
| Isolation | No records are linked across unintended tenants or companies |

Capture opening balances; compare scenario deltas rather than assuming the database is empty. Group multi-currency checks by transaction currency and also reconcile accounting currency. Account for discounts, exchange differences, credit notes and reversals explicitly.

## Evidence report

Record run ID, company, seed versions, scenario/step status, document IDs, voucher IDs, expected/actual values, tolerance, duration and errors. Separate pass, fail, blocked and not-run states. Include the root error and responsible service without sensitive payloads.

A completed report requires all enabled scenarios to pass. If functionality is unsupported, list it as blocked with a concrete implementation gap. Documentation completion is not proof of functional completion.
