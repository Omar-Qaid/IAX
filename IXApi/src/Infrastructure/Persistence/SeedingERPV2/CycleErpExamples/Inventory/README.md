# Inventory examples

`Logic_Readonly.md` is the acceptance reference and is not modified by this package.

The package follows the Accounts Payable split, file for file:

| Concern | Here | In Accounts Payable |
| --- | --- | --- |
| Master writes | `Modules/Inventory` seeders | `Modules/AccountsPayable` seeders |
| Fixture facts | `InventoryCycleSeedExample` | `InventorySeedExample`, `VendorSeedExample` |
| Manifests, checked against the seeders | `Data/InventoryExampleCatalog` + `items.json`, `inventory-setup.json`, `scenarios.json` | `AccountsPayableExampleCatalog` + its four manifests |
| Business keys | `Data/ExampleIdentifiers` | `Data/ExampleIdentifiers` |
| Preparation | `InventoryCycleSeeder.PrepareAsync` | `AccountsPayableCycleSeeder.PrepareAsync` |
| Preparation validation and reference capture | `Validation/InventoryCycleValidator` | `Validation/AccountsPayableCycleValidator` |
| Scenario execution | `InventoryTransactionExamples` → `InventoryTransactionFlow` | the same pair |
| Posted-state reconciliation | `Validation/InventoryReconciliation` + ten `IInventoryCycleCheck` classes | `AccountsPayableReconciliation` + ten `IAccountsPayableCycleCheck` classes |
| Reports | `Reports/InventoryExecutionReport`, `Reports/InventoryReconciliationReport` | the equivalent three |

Preparation creates no stock, invoice or accounting entry. Reports contain actual persisted quantities and values; a
failed check throws, so it cannot produce a successful report or a completed-scenario history record.

## Running it

```csharp
// Preparation only.
services.AddInventoryConfigurationAndMasterDataExamples();
var prepared = await scope.ServiceProvider.GetRequiredService<InventoryCycleSeeder>()
    .PrepareAsync(new InventoryExampleOptions(2026, actorUserId), ct);

// Transactions.
services.AddInventoryTransactionExamples();
var report = await scope.ServiceProvider.GetRequiredService<InventoryTransactionExamples>()
    .RunAsync(new InventoryTransactionOptions(2026, actorUserId, ["01"]), ct);
```

Omitting the scenario ids selects every implemented scenario. Supply `ApplicationDbContext`, the company context,
the current user and the host's Finance mappings as in the AP and AR packages. Neither registration seeds anything
at application startup.

Preparation inserts missing master rows and rejects conflicting existing values, preserving consumed sequence
counters. Documents use stable scenario identifiers, so reruns verify existing lines and reconcile a completed close
rather than posting a second cycle.

## Coverage, stated plainly

`InventoryTransactionFlow.Scenarios` includes `01`, the reference purchase / reservation /
partial sale / service / second purchase / weighted-average close flow, expecting 90 on hand, inventory value 1,080,
cost of goods sold 720 and a close adjustment of -120.

Scenario `02` executes positive and negative adjustment journals on a separate item. It expects 8 on hand, value
80, profit credited 100, loss debited 20, and zero cost of goods sold. Its isolated verification result is recorded
under `ADJ-01` in the acceptance register; implemented code alone does not certify a case.

Scenario `03` generates a counting journal from an independent opening balance of 90 at cost 12, freezes and
confirms a count of 88, then reconciles the -2 difference, inventory value 1,056 and loss expense 24. Consult
`CNT-01` in the acceptance register for verification evidence.

Scenarios `04` and `05` are the overage (93) and matching (90) counts. `06` is a transfer order with a partial
receipt, `07` a same-site transfer journal, `08` a movement journal with an explicit offset account, and `09` a
weighted-average recalculation that leaves the period open. `10` is the catalog's INV-21 FIFO close, which
adjusts a running-average sale of 150 from 9,450 to 9,300 and leaves 50 at 66.

[Scenarios/README.md](Scenarios/README.md) maps each catalog row (INV-01 … INV-30 in `AllModules/Secnarios.md`,
I1 … I5 in `AllModules/FullExample.md`) to its seeded scenario or covering test, and lists the rows not seeded.
Do not read a successful preparation or green scenarios as full Inventory acceptance. `Data/acceptance-status.json`
is the authority per case id, and `PHASE-TWO.md` records what remains.

## Documentation

`Documentation/01-Scope.md` through `09-ExecutionAndRerun.md` cover scope, dependencies, configuration, master data,
the transaction flow, the costing arithmetic, impacted tables, reconciliation and rerun behavior.
`Scenarios/01-ReferenceCycle/` holds that scenario's own README, expected results, accounting entries and impacted
tables.

## Tests and evidence

`InventorySeedIntegrityTests` covers master preparation, reruns, conflict preservation and the real-model
transaction package. `InventoryCycleTests` holds
`Inventory_reference_cycle_closes_at_ninety_on_hand_and_one_thousand_eighty_value`, which drives the same eleven
steps through the real services. `InventoryAcceptanceTests` retains the physical-value regression cases. Run logs
and TRX results are kept under `.validation/inventory-package` and `.validation/inventory-20261109`, including
failing attempts.
