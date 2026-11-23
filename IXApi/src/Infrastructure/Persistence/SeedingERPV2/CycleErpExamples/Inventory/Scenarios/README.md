# Inventory scenarios against the catalog

Sources: `AllModules/Secnarios.md` §3 (INV-01 … INV-30), `AllModules/FullExample.md` part 5 (I1 … I5) and
`Logic_Readonly.md` §2. A catalog row counts as **seeded** only when an `InventoryTransactionFlow.Scenarios` entry
posts it through the owning services. **Test only** means a service test proves the behavior, but no seeded
example exists. `Data/acceptance-status.json` remains the authority for certification.

## Seeded scenarios

| Seeded | Folder | Catalog rows | FullExample |
| --- | --- | --- | --- |
| 01 | [01-ReferenceCycle](01-ReferenceCycle/README.md) | INV-20 (weighted-average close), INV-25 (service line), `Logic_Readonly.md` §2 | I5 |
| 02 | [02-Adjustment](02-Adjustment/README.md) | INV-11, INV-12 | — |
| 03 | [03-Counting](03-Counting/README.md) | INV-14 | I4 |
| 04 | [04-05-CountingVariants](04-05-CountingVariants/README.md) | INV-13 | I4 (positive variant) |
| 05 | [04-05-CountingVariants](04-05-CountingVariants/README.md) | INV-15 | — |
| 06 | [06-TransferOrder](06-TransferOrder/README.md) | INV-02, INV-04 | I1 |
| 07 | [07-TransferJournal](07-TransferJournal/README.md) | INV-07 | I2 |
| 08 | [08-Movement](08-Movement/README.md) | INV-09, INV-10 | I3 |
| 09 | [09-Recalculation](09-Recalculation/README.md) | INV-22 | — |
| 10 | [10-FifoClose](10-FifoClose/README.md) | INV-21 | — |

GL-02 (`GeneralLedger/Scenarios/02-ReferenceCycle`) also runs an inventory close on its own `GLREF` item.

## Catalog rows not seeded

| Catalog | Status | Evidence or note |
| --- | --- | --- |
| INV-01 opening balance | Partial | Scenarios 03–07 open stock through an adjustment journal. No movement journal against an opening-balance offset account is seeded. |
| INV-03 partial shipment | Not seeded | Scenario 06 ships the full 10 and splits only the receipt. |
| INV-05 cancel before shipment | Not seeded | — |
| INV-06, INV-08 cross-site transfer | Not seeded | Every example uses the single `APV2-SITE`. |
| INV-16 ⚙️ count locking | Not seeded | Optional. |
| INV-17 ⚙️ inventory blocking | Not covered | No `InventBlocking` test. |
| INV-18 negative inventory | Test only | `OrderToCashAcceptanceTests`. |
| INV-19 posting before the last close | Test only | `InventoryCycleTests`. |
| INV-23 cancel closing | Test only | `InventoryCycleTests`, `InventoryWeightedAverageReturnCloseTests`. |
| INV-24 invoice price variance after sale | Not seeded | — |
| INV-26 received, not invoiced value | Test only | `InventoryAcceptanceTests` (physical value), `InventoryValueReconciliationTests`. |
| INV-27 ⚙️ standard cost | Not covered | Optional. |
| INV-28 … INV-30 reports | Partial | Each seeded scenario reconciles `InventSum`, `InventTrans` and inventory value against GL. No report output is seeded. |

The catalog's negative rows NEG-10 … NEG-12 are not seeded. NEG-12 is the same rule as INV-19.
