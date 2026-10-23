# Basic inventory setup (Phases 0–2) — implementation status

Updated 2026-10-23. Scope of this review is configuration and released-item setup only. No operational stock, reservation, settlement, customer/vendor balance, or ledger posting was added or performed.

| Setup feature | Status | Evidence and remaining gate |
| --- | --- | --- |
| Phase 0 architecture and schema | Partial | Repository architecture, EF entities, company context, and existing setup services are recorded in `Basic-Inventory-Phase-0-Assessment.md`. The copied SQL Server schema, enum storage, keys, precision, and relationships have not been compared with the deployed database. |
| Units and conversions | Partial | Existing CRUD now rejects case-insensitive duplicate symbols, opposite-direction duplicate conversions per scope, cross-class conversions, offsets, and ambiguous factor/fraction input. Referenced units cannot change math-relevant settings. Database-enforced uniqueness and concurrent update checks remain open. |
| Sites and warehouses | Partial | Existing company-scoped services protect referenced records and site relationships. SQL Server reference coverage remains unverified. |
| Dimension groups and FIFO model groups | Partial | Existing basic site/warehouse, NONE tracking, and FIFO restrictions are implemented in the Phase 1 pass. Copied schema and used-configuration tests remain open. |
| Item groups, posting profiles, journal names, parameters, sequences | Partial | Existing services and readiness checks cover core setup. Full account precedence, numbering concurrency, and schema-backed uniqueness remain open. |
| Shared products and company release | Partial | Existing services support distinct Item creation, translations, release, assignments, and readiness. Atomic create/release and company isolation need SQL Server verification. |
| Module units, conversions, and order defaults | Partial | Setup services and readiness exist. Product conversion readiness now rejects missing units and unsupported offset rules. One rounding policy across all consumers remains open. |
| Barcodes and categories | Partial | Barcode lookup preserves text including leading zeros, resolves inventory quantity from a factor or fraction, and rejects ambiguous company mapping. Simple category assignment exists. Barcode setup linkage and SQL uniqueness remain unverified. |

## Changes in the setup-only pass

- `UnitOfMeasureService`: validates supported linear factors/fractions and unit classes; protects referenced units during update/delete. Reads `UnitOfMeasure`, `UnitOfMeasureConversion`, `EcoResProduct`, `InventTableModule`, `InventItemBarcode`, `InventParameters`, and `InventJournalTrans`; writes only unit and conversion setup records.
- `ReleasedProductListService`: conversion readiness requires existing units and a supported linear conversion. This pass is read-only for those checks.
- `InventItemBarcodeService`: lookup and validation use the same factor-or-fraction interpretation and reject offsets. This pass changes calculation only.
- `InventoryArchitectureTests`: added unit, reverse-duplicate, fraction, barcode, leading-zero, and blocked-barcode cases. Focused SQLite test result: **3 passed, 0 failed**. Finance module build: **passed, 0 errors**.

Existing `UnitOfMeasure` and released-product API routes and React pages remain in place; no new route or page was needed for these fixes. The tests are SQLite fixtures and do not prove SQL Server concurrency, database constraints, or migration compatibility. Phases 0–2 remain **incomplete** until those gates and the remaining Phase 1/2 status items are resolved.
