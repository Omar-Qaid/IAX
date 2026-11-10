# MD-04 tracking dimensions — outcome

No test job was active before this work started: `package-20261110-regression-exit.json` was already written
(`FullExitCode 1`, `CycleExitCode 0`, finished 00:49), so the earlier `*-active.md` notes were completed handoffs
rather than a live run. The `SecurityConfigurationTests` failure recorded there is repaired and verified
(`package-20261110-security-path-fixed.trx`, 3 of 3 passed, exit 0).

## What was missing

MD-04 had no evidence and no implementation. The cause was not the seed data. `InventDimensionResolver` refused
every tracking dimension with `Only site and warehouse dimensions are supported in the basic inventory scope.`,
while everything beneath that guard — the canonical dimension hash, the uniqueness lookup and the `InventDim`
row itself — already carried batch and serial. The purchase path had no way to state either: `PurchLineInputDto`
had no such field and `PurchOrderService` passed only site and warehouse to `PurchInventorySupplyService`.

Batch and serial tracking are in scope by explicit user authorization. That authorization is what permits the
guard to be relaxed; it is recorded here because the message it raised is a deliberate restriction.

## Changes

| Component | Change |
| --- | --- |
| `InventDimensionResolver` | The guard admits batch and serial only. Product, status, WMS and license-plate dimensions are still refused, and the message now reads `Only site, warehouse, batch, and serial dimensions are supported in the basic inventory scope.` |
| `PurchLineInputDto` | Gained `InventBatchId` and `InventSerialId`, both defaulted, so existing callers are unchanged |
| `IPurchInventorySupplyService`, `PurchInventorySupplyService` | Carry the two dimensions and build a template only when one is supplied, so an untracked line resolves to the same dimension it always did |
| `PurchOrderService` | Applies `InventoryDimensionPolicy` — the same policy `InventJournalPostingService` uses — before the line is created, so a tracked item cannot receive a blank or undefined batch or serial, and an untracked item cannot record one |
| Inventory module seeders (8 files) | `APV2-BATCH` and `APV2-SERIAL` stocked items, with `IV2-{company}-BATCH` and `IV2-{company}-SERIAL` tracking groups. Each group activates exactly one tracking field, so neither fixture constrains the other and `APV2-STOCK` stays untracked |
| `CycleContext` (tests) | `InventBatch` and `InventSerial` registered; a stated batch or serial is checked against those defined for the item |

## Outcome

`md04-tracking-batch-and-serial.trx`: **8 passed, 0 failed, 0 skipped**, isolated SQLite real-model database.

| Case | Expected | Actual |
| --- | --- | --- |
| Two batch receipts, same item/site/warehouse | Two distinct `InventDim` rows and two `InventSum` rows, 10 and 6 | As expected |
| Packing slip naming `B-001` for 4 | `B-001` on-hand 10 → 6, `B-002` unchanged at 6, issue on the receipt's dimension | As expected |
| Blank batch on a tracked item | Refused, no `PurchLine` written | `A batch number is required for item 'ITEM-001'.` |
| Undefined batch `B-404` | Refused, no `PurchLine` written | `Batch 'B-404' is not defined for item 'ITEM-001'.` |
| Batch on an untracked item | Refused | `Item 'ITEM-001' has no tracking dimension group, so a batch or serial number cannot be recorded.` |
| `ConfigId` through the resolver | Still refused; a batch resolves | As expected |
| Two serial receipts of one unit | Two dimensions, each on-hand 1, batch left blank | As expected |
| Serial line of quantity 3, blank serial, undefined serial | All three refused | `Each line for serial-tracked item 'ITEM-001' must identify one serial-numbered unit.`, `A serial number is required for item 'ITEM-001'.`, `Serial number 'SN-404' is not defined for item 'ITEM-001'.` |

Two defects were found and fixed during the work, both in the new code rather than in the product:
`EcoResProductSeeder` still keyed `ProductType` off `suffix == "STOCK"`, which would have created the batch
product as a service; and the test helper built `PurchaseServices` per receipt, so two independent number
sequences handed both orders the same `PurchId`.

## Regression

`md04-regression-full.trx` (batch only): 938 passed, 1 failed, 18 skipped of 957.
`md04-regression-cycle.trx`: 146 passed, 0 failed.

The single failure is `AccountsPayableGenericArchitectureTests.Accounts_payable_features_adhere_to_seven_folder_layout`.
It is not caused by this work. `AccountsPayable/Registration/` arrived with commit `1cdc0ef5`
(*feat(ap): arrival registration*) carrying only `Dtos`, `Interfaces` and `Services`, and is missing
`Configuration`, `Controllers`, `Entities` and `Validation`. It is new since the 00:49 run, which predates that
commit, and it is outside the Inventory scope. Creating empty folders would not fix it, because git does not
track empty directories; either the feature is incomplete or the rule does not fit a service-only feature, and
that belongs to the author of `1cdc0ef5`. It is recorded here as a known failure, not repaired.

The serial change landed after that regression, so a second full run was taken after it.
`md04-serial-regression-full.trx`: 940 passed, 1 failed, 18 skipped of 959, duration 14m11s.
`md04-serial-regression-cycle.trx`: 146 passed, 0 failed. The passed count rises by exactly the two serial tests,
and the one failure is the same unrelated AccountsPayable layout test. Both stages are therefore clean for this
work, and the 18 skips are unchanged.

## Not certified by this run

Serial tracking has no sales-issue scenario: receipt, per-serial on-hand and the one-unit rule are verified, the
packing-slip path is verified for batch only. Both full-regression stages are recorded above, so the batch and
serial service changes are regression-clean; what remains unverified for serial is the issue scenario itself. The two share one resolver, so this is a scenario gap, not a
service gap. SQL Server behavior, concurrency and the 18 opt-in skips remain unverified. The register stands at
8 of 40 cases passed; 32 in-scope cases remain, so the recurring automation must stay active.
