# 04 — Master data

Ten released products per fiscal year, described in `Data/items.json`. The two reference-cycle products below
are joined by `IV2-{year}-ADJ`, which has the same stocked policy and module prices as STOCK but independent
transactions for adjustment scenario 02. It is owned by InventoryCycleMasterSeeder and validated from the manifest.

`IV2-{year}-COUNT` uses the same stocked policy for counting scenario 03. Its opening adjustment creates 90 units
at cost 12 through the journal service; counting this independent item cannot change another scenario's balances.

`IV2-{year}-CPLUS` and `IV2-{year}-CMATCH` isolate the overage and matching-count scenarios 04 and 05.
Each starts with its own 90 units at cost 12; expected counted quantities are 93 and 90 respectively.

`IV2-{year}-TRANS` isolates transfer-order scenario 06 with its own opening 90 units at cost 12.

`IV2-{year}-TJOUR` similarly isolates same-site transfer-journal scenario 07.

`IV2-{year}-MOVE` isolates movement-journal receipt and issue in scenario 08.

`IV2-{year}-RECALC` isolates weighted-average recalculation and post-recalculation entry in scenario 09.

| | Stocked item | Service item |
| --- | --- | --- |
| Item id | `IV2-{year}-STOCK` | `IV2-{year}-SVC` |
| Product type | `Item` | `Service` |
| Item model group | `APV2` (stocked, weighted average) | `APV2-SVC` (non-stocked) |
| Item group | `APV2` | `APV2` |
| Unit | `EA` | `EA` |
| Item tax group | `IV2-ZERO` | `IV2-ZERO` |
| Purchase price | 10 | 300 |
| Sales price | 25 | 300 |
| Storage / tracking dimension group | assigned | not assigned |
| Default order settings | present | not present |

`APV2-BATCH` is the module-owned batch-tracked item behind MD-04. It is a stocked item on the same
`APV2` model and item groups as `APV2-STOCK` and shares its storage dimension group, so the only
difference between them is the tracking group: `APV2-{company}-NONE` leaves batch and serial
inactive, while `IV2-{company}-BATCH` activates the batch number alone. An item carries exactly one
tracking group, so a second item is the only way to keep the untracked scenarios’ figures intact.
Serial stays inactive on both groups; a serial-tracked fixture remains unseeded.

The service item deliberately has no dimension groups and no order settings: a non-stocked product has no stock to
locate. `NonStockedItemValidator` then asserts the consequence — it raises no `InventTrans` row at all — because a
movement raised for it would be absorbed into the stocked item's period average and quietly change the closing value.
