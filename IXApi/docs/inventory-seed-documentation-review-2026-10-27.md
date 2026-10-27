# Inventory documentation versus seed logic - 2026-10-27

Scope: source review of Inventory/CycleInvnetory.md, InTables.md, Note.md, README.md, Phase 11 status, and the registered inventory/product/purchase/sales example seeders. No business records or application code were changed by this review.

## Conclusion

Partially aligned, not the complete documented inventory cycle. Populating every existing table does not mean every blueprint scenario is seeded. The blueprint is a target; older implementation-status Markdown also contains statements superseded by current services.

| Requirement | Current seed behavior | Assessment |
| --- | --- | --- |
| Shared product versus company release | Shared product/translation and released-item/module/group links are created separately | Aligned |
| Posting engine owns transactions, stock summaries and vouchers | Scenario seeders call journal, purchase receipt/invoice and sales delivery/invoice services; no direct InventSum balance fabrication was found in seeding | Aligned |
| FIFO stocked baseline, both negative-inventory flags disabled | InventorySeeder sets FIFO NegativeFinancial=Yes (line 84), contrary to CycleInvnetory.md lines 393-397 | Confirmed configuration mismatch; existing group must not be silently overwritten |
| Opening +100 at 50; purchase +50 at 60 | Inventory example posts ITM-1002 +10/-2 PCK at 20. Purchase examples use ITEM-001 quantities 10/20/30 at 100, with open/partial/full states | Valid smaller examples, not the prescribed numerical scenario |
| FIFO layers with different costs and nonzero adjustment | Journal example has a single cost layer; close is exercised but no differing-layer cost adjustment case | Incomplete scenario coverage |
| Close across purchase and sales inventory | Current InventCloseService rejects non-journal origins and transfer origins; example closes historical journal-only transactions before current purchase/sales dates | Supported limited close, not full blueprint FIFO settlement |
| Physical reservation and picking | Sales demand/delivery examples exist; seeders do not explicitly exercise reserve/release/pick operations | Missing examples |
| Transfer journal / transfer order | Journal names/setup exist, but InventoryScenarioSeeder creates only adjustment receipt/issue and draft counting | Missing execution examples; transfer-order schema/flow must be verified before implementation |
| Counting with posted discrepancy | Count of 9 against 8 creates an unposted count; service intentionally blocks posting without confirmed count/movement freeze | Correct guard preserved, blueprint acceptance incomplete |
| Customer/vendor returns | Return reason/disposition setup exists; no return document scenario is seeded | Missing execution examples |
| Batch/serial and blocked lifecycle/status cases | Setup/master records exist, but no transactional examples exercise them | Setup coverage only |
| Standard cost | DEMO-PLAN is a planned cost record, not activation/revaluation | Does not prove standard-cost behavior |
| Inventory-to-GL reconciliation | Balanced-voucher and quantity checks exist | Not full inventory-value/account/dimension/cutoff reconciliation |

## Documentation drift

Inventory/README.md still says purchase execution and inventory close are absent/deferred. Current PurchaseScenarioSeeder invokes real purchase services; InventCloseService implements a restricted journal-only FIFO close. Historical status notes should remain historical, with current scope clarified rather than treating them as current prohibitions or proof of full completion. InTables.md explicitly calls itself a functional checklist rather than a verified D365 schema export.

## Safe alignment order

1. Establish a dedicated blueprint example item/model group with negative physical and financial inventory disabled, without changing customized or already-used group policy.
2. Add supported opening, movement, transfer-journal and differing-cost FIFO cases through existing services, using isolated identifiers and repeatable execution.
3. Track reservations/picking, protected posted counts, returns, transfer orders, cross-document close and financial reconciliation as implementation/acceptance gaps until their services and schema support the documented behavior.
4. Verify quantities and financial values against the specific blueprint scenario, not table counts alone.

This review does not invalidate the earlier database constraint checks. It distinguishes relational integrity from complete business-scenario and documentation parity. It does not certify every inventory workflow or a clean-database installation.
