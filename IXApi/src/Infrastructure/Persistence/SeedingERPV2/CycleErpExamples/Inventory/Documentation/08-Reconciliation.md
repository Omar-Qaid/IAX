# 08 — Reconciliation

`InventoryReconciliationSnapshot` loads the posted state once, after a scenario finishes. Every amount and quantity
is read back from the persisted rows; nothing in the snapshot recomputes what a posting or closing service decided.

Each check owns exactly the concern its name states:

| Check | Concern |
| --- | --- |
| `OnHandQuantityValidator` | the scenario leaves the on-hand quantity it set out to leave |
| `InventSumConsistencyValidator` | the summary agrees with its movements, and available = on hand - reserved |
| `InventoryValueValidator` | financial inventory matches the expectation and the inventory GL account |
| `CostOfGoodsSoldValidator` | the issue side matches the expectation and the COGS account |
| `CloseAdjustmentValidator` | the close moved the expected adjustment, and actually ran |
| `InventSettlementValidator` | each cost settlement names its movement and agrees with its adjustment |
| `ReservationValidator` | reserved is as expected and never exceeds on hand |
| `NonStockedItemValidator` | the service raised no stock movement |
| `TransactionOriginValidator` | no movement is orphaned from its source document |
| `LedgerBalanceValidator` | something was posted, and every voucher balances in both currencies |

`LedgerAmount` throws on a main account that is not in the seeded demo chart rather than returning zero. A zero
would let a renumbered chart satisfy a check that expects a cleared account.

A failed check throws. It cannot produce a successful report, and `InventoryTransactionExamples` records scenario
history only after the scenario returns, so a failure leaves no completed-run marker behind.
