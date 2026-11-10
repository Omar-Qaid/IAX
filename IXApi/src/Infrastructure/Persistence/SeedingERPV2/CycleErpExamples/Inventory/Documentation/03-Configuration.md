# 03 — Configuration

Everything here is written by an `IDataSeeder` under `Modules/Inventory`, described in
`Data/inventory-setup.json`, and checked by `Validation/InventoryCycleValidator`.

## Costing

Both item model groups use `InventModel.WeightedAvg`. The stocked group has `StockedProduct = Yes`,
`PostOnHandPhysical = Yes`, `PostOnHandFinancial = Yes` and `PostPhysicalPurchase = Yes`; the service group has
`StockedProduct = No`. Any other costing method invalidates every figure in this package, so the catalog refuses
a manifest that names one.

## Posting profiles

`InventPostingSeeder` writes five `InventPosting` rows against item group `APV2`:

| Account type | Main account |
| --- | --- |
| `PurchPackingSlip` | 181003 |
| `PurchPackingSlipOffsetAccount` | 281002 |
| `PurchReceipt` | 181002 |
| `PurchConsump` | 681001 |
| `PurchDisc` | 481001 |

These five constants live on `InventoryCycleSeedExample.PostingProfiles`, so the seeder that writes them and the
manifest that describes them cannot disagree.

## Tax

The item tax group is `IV2-ZERO`, a zero rate. Costing is the subject of this cycle; a non-zero rate would add tax
amounts to the invoice totals without changing inventory value, which only makes an assertion failure harder to read.

## Numbering

`InventoryNumberSequenceSeeder` provides the `InventClosing` and `SettleTransId` sequences and refuses an exhausted
one. Consumed counters are never rewound on a rerun.
