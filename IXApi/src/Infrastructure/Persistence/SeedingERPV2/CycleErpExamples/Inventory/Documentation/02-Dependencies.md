# 02 — Dependencies

Inventory does not own vendors, customers, the ledger, tax or dimensions. It reuses the records the AP and AR
example seeders already own, and fails loudly if one is absent rather than creating a second copy.

| Needed for | Record | Owned by |
| --- | --- | --- |
| Buying the stock | Vendor `APV2-{company}-LOCAL`, purchase parameters, matching policy | `Modules/AccountsPayable` |
| Selling the stock | Customer `ARV2-{company}-LOCAL`, sales parameters | `Modules/AccountsReceivable` |
| Posting | Ledger `APV2`, chart of accounts, main accounts, dimension combinations | `Modules/GeneralLedger` |
| Zero-rate tax | Item tax group `IV2-ZERO` | `Modules/Tax/InventoryExampleTaxSeeder` |
| Stock location | Site `APV2-SITE`, warehouse `APV2-WH`, dimension `APV2-DIM` | `Modules/Inventory` |
| Costing | Item model groups `APV2` and `APV2-SVC`, posting profiles on item group `APV2` | `Modules/Inventory` |

`InventoryCycleRegistration` therefore registers the AR phase-one seeding (which pulls AP and GL behind it) before
the Inventory cycle seeders, and `SeedRunner` resolves the declared `IDataSeeder` dependency graph.

## Why the items are scoped by fiscal year

`InventoryCycleMasterSeeder` names its items `IV2-{year}-STOCK` and `IV2-{year}-SVC`. The reference cycle asserts an
exact closing value, which is a function of every movement on the item in the period. Sharing an item with the AP or
AR demonstrations, or with a previous year's close, would make that assertion depend on run order.
