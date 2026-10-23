# Basic inventory — Phase 2 implementation status

Date: 2026-10-23. This pass changed product and released-item setup only. It made no `InventTrans`, `InventSum`, settlement, or ledger writes. The authoritative copied SQL schema is still unverified, so this is **not** a Phase 2 completion certificate.

## Implemented in this pass

- Limited new products and company releases to distinct `Item` products. Shared product and company item numbers remain separate. The combined create-and-release operation now requires group, unit, and order settings; the separate release operation remains an explicit incomplete draft until configured.
- Kept shared product creation and its first translation in one transaction; made the Arabic translation optional in the direct product API. Existing translation updates still upsert by language.
- Validated stocked FIFO model assignment, site/warehouse storage dimensions with site-level financial tracking, and inactive batch/serial tracking on configuration and group assignment.
- Blocked inventory-unit changes after an item has inventory transactions.
- Prevented a company barcode from mapping to multiple items or units. Barcode lookup now returns converted inventory quantity and preserves the scanned text value, including leading zeros.
- Added `GET /api/v1/InventTable/released-products/{recId}/readiness` with separate purchase, sales, and inventory-journal results. It checks the core product, group, unit, conversion, default warehouse, stopped, and posting-account setup without writing stock.
- Updated the product creation screens to show the supported Item type and require complete configuration for combined creation.

## Remaining before Phase 2 exit

1. Verify all product, assignment, module, conversion, barcode, and default-order tables and enum values against the copied SQL schema. Phase 0 remains incomplete.
2. Run the complete Phase 2 acceptance cases against a disposable SQL Server database, including concurrent duplicate keys, transaction rollback, cross-company release, and no stock/ledger effects.
3. Verify product translation language rules and fallback against the application's supported language configuration. The current direct product API uses English and optional Arabic.
4. Confirm the barcode setup/format relationship in the copied schema. The mapped `InventItemBarcode` entity currently exposes barcode text, quantity, unit, and dimension but no barcode setup reference.
5. Consolidate conversion calculations into one shared service so release validation, barcode preview, journals, and later purchase/sales posting follow one precision and rounding policy.
6. Complete operation-specific readiness for number-sequence references and all source-document posting mappings. The new endpoint checks core account categories; later source integrations remain separate phases.
7. Confirm behavior for site-specific default-order overrides, lifecycle states, stopped corrections, and returns before those flows are enabled.

Existing separate release/configure operations can still create an incomplete draft. Posting services must reject an item that fails the applicable readiness check.
