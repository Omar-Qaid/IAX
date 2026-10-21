# Inventory backend architecture

## Phase A basic ERP foundation (2026-10-21)

The backend foundation includes shared product category hierarchy and assignment APIs, released-item barcode maintenance, and a simple product lifecycle status assigned on `InventTable` with an optional default on release. The approved EF migration `AddBasicInventoryCategoriesAndProductStatus` is code only and has not been applied to SQL Server. Product and item number-sequence definitions are seeded through the existing shared `SysNumberSequence` service; product creation still accepts manually entered numbers. Warehouse inventory status (`WHSInventStatus`) is excluded from this basic inventory scope. Phase B stocked/service behavior and Sales/Purchase integration are separate checkpoints.

## Existing feature coverage

| Feature | Architecture |
| --- | --- |
| InventSite | Thin controller, IBaseService contract, BaseService with IUnitOfWork, Mapster configuration, update validator. |
| InventLocation | Thin controller, IBaseService contract, BaseService with IUnitOfWork, Mapster request/response configuration, update validator. Reference existence and deletion guards remain service business rules. |
| InventTable / InventTrans list | Thin controller, query service interface, IUnitOfWork data access, Mapster configuration, separate list DTO. The service owns company checks, joins and total cost calculation. |
| ItemGroup and InventTable DTO validators | Existing validators relocated from Dtos to Validation without changing rules or namespaces. |
| InventSum on-hand | Read-only company-scoped inquiry joins `InventSum`, `InventTable`, and `InventDim`; exposes basic site/warehouse quantities and values. |
| UnitOfMeasure / UnitOfMeasureConversion | Shared UOM setup supports master CRUD plus global or product-specific conversion CRUD, duplicate validation, positive factors, and used-record restrictions. |
| InventBatch, InventClosing, InventCountGroup, InventItemLocation, InventJournal, InventPackaging, InventSerial and InventSettlement | Persistence types only. They are not treated as working business processes until services, posting logic, permissions, forms, and executed tests exist. |

Controllers retain their existing custom routes and response contracts. The shared BaseController introduces additional generic routes and different CRUD contracts, so these existing custom controllers continue to use ControllerBase. Warehouse and site services reuse BaseService/IBaseService. All Inventory service persistence now flows through IUnitOfWork.

Feature files are grouped under Controllers, Interfaces, Services, Dtos, Configuration, Validation and Entities where those responsibilities exist. Existing entity classes, EF mappings, permissions and company-filter behavior are preserved.

Warehouse lookup projections remain database-side projections of scalar fields. Mapster handles warehouse request/entity/response mapping and inventory transaction list mapping. Warehouse writes preserve normalization, immutable business codes, same-site reference checks and delete guards.

The basic UI intentionally hides WHS, production, retail, quality-management, and advanced planning warehouse fields without removing their stored columns. Basic warehouse maintenance shows warehouse, name, owning site, and standard warehouse type.

## Phase 1 baseline and D365 basic-core gap matrix

Reference baseline: current Microsoft Dynamics 365 Supply Chain Management online documentation reviewed on 2026-10-20. The repository doesn't declare a specific D365 application version, so exact version parity is not claimed. Object names below are standard tables/forms or standard concepts, not data-entity names unless explicitly stated.

| Area | Existing implementation | D365 F&O equivalent | Classification / gap | Required change | Priority | Verification source |
| --- | --- | --- | --- | --- | --- | --- |
| Legal entity context | `DataAreaId`, company middleware/context and company-scoped services | Legal entity/company context | Working and aligned for reviewed services; every new journal query must still apply it explicitly | Preserve server-side company enforcement and add cross-company journal tests | Critical | Repository middleware and service tests |
| Shared product | Product API/pages and EF mappings | `EcoResProduct`, `EcoResDistinctProduct`, `EcoResProductTranslation`; Products V2 data entity is `EcoResProductV2Entity` | Simple distinct products; Item/Service choice is wired through both creation forms and APIs; unsupported types are rejected. Status/lifecycle maintenance is missing; variants remain excluded | Add status only if its exact standard mapping and in-scope behavior are verified | High | [Product information overview](https://learn.microsoft.com/en-us/dynamics365/supply-chain/pim/product-information); [product identifiers](https://learn.microsoft.com/en-us/dynamics365/supply-chain/pim/product-identifiers) |
| Released product | Release API/page and released-product details | `InventTable`; Released products V2 data entity is `EcoResReleasedProductV2Entity` | Company release preserves `InventTable.Product -> EcoResProduct.RecId`; readiness accepts either stocked setting; sales demand now follows the assigned model group's `StockedProduct` flag when an assignment exists. Purchase-order functionality is absent from this repository and remains an external dependency | Add tests for service readiness and verify sales downstream posting; don't claim purchase-service behavior until a purchase-line flow exists | Critical | [Product information overview](https://learn.microsoft.com/en-us/dynamics365/supply-chain/pim/product-information); [inventory costing FAQ](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-costing-faq) |
| Product categories | `EcoResCategory` exists in posting lookup only | `EcoResCategory`, `EcoResCategoryHierarchy`, `EcoResProductCategory` (category assignment); category assignment data entity is `EcoResProductCategoryAssignmentEntity` | Category master/hierarchy assignment UI/API is missing; current posting Category relation is not product categorization | Add basic category hierarchy/assignment only after confirming required table mappings and constraints; do not reuse posting-profile relations as product classification | Medium | [Product category assignment entity metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/entities/supplychain/productinformationmanagement/ecoresproductcategoryassignmententity) |
| Product identifiers/barcodes | `InventItemBarcode` entity, company-scoped CRUD API, and released-product maintenance dialog | Released item barcodes; standard Item - barcodes V3 entity is export-only, and D365 uses the Item - barcode association entity for updates/deletes | Basic maintenance validates released item, unit, optional dimension, quantity, and duplicate item/unit/dimension associations. D365 permits one barcode on multiple products. Barcode setup and retail variants remain unsupported; concurrent uniqueness needs a database constraint | Verify authenticated UI/API and persistence later; define barcode-setup entity and uniqueness constraint only with schema approval | Medium | [Product data entities](https://learn.microsoft.com/en-us/dynamics365/supply-chain/pim/data-entities); [product identifiers](https://learn.microsoft.com/en-us/dynamics365/supply-chain/pim/product-identifiers) |
| Released-item setup | Units, commercial defaults, group assignments and default-order settings | `InventTableModule`, `InventItemGroupItem`, `InventModelGroupItem`, `EcoRes*DimensionGroupItem`, `InventItem*Setup` | Present and substantially implemented | Test all server validations and used-record restrictions | High | Repository service mappings and released-product tests |
| Inventory parameters | Company-scoped GET/PUT API and `SetupPage` UI | `InventParameters` and Inventory and warehouse management parameters form | Implemented basic surface; no number-sequence reference surface yet | Apply migration; connect verified inventory journal sequence references during journal phase | High | Microsoft `InventParameters` CDM metadata and number-sequence documentation |
| Units and conversions | Shared UOM and conversion CRUD with list-details maintenance UI | `UnitOfMeasure`, `UnitOfMeasureConversion` | Basic maintenance and a 1 box = 4 pcs journal conversion are covered; reverse conversion and rounding boundaries remain unverified | Add reverse and non-integer rounding execution tests | High | Repository UOM and inventory journal tests; Microsoft unit conversion behavior |
| Sites and warehouses | CRUD pages/APIs with company validation | `InventSite`, `InventLocation` | Working and aligned for basic scope | Retain company and owning-site restrictions | High | Repository inventory architecture tests |
| Basic inventory locations | Basic module API/page, used-dimension guards, EF model mapping, and versioned migration | `WMSLocation` | Migration is generated but not applied; SQL Server behavior is unverified | Apply migration in an authorized test DB and verify basic location dimensions; exclude WHS execution | High | [Microsoft inventory locations](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-locations); [WMSLocation CDM metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/main/wmslocation) |
| Item and model groups | Setup pages/APIs and assignment tables | `InventItemGroup`, `InventItemGroupItem`, `InventModelGroup`, `InventModelGroupItem` | Present; transactional use restrictions need broader coverage | Add referenced-record delete/update tests | High | Repository services and D365 released-product setup behavior |
| Storage/tracking groups | Setup and assignment pages/APIs | `EcoResStorageDimensionGroup*`, `EcoResTrackingDimensionGroup*` | Basic setup present; optional batch/serial behavior not yet transaction-tested | Enforce active/blank dimension rules in journal line validation | Critical | Microsoft inventory dimension documentation |
| Inventory status | `InventDim.InventStatusId` field is present | Inventory status dimension/table and applicable lookup behavior | Dimension value is stored but status setup/lookup/validation is not established; exact applicability depends on enabled dimension configuration | Verify standard table/version behavior; only expose statuses when the dimension is enabled | Medium | D365 inventory-dimension metadata required; not verified in local environment |
| Journal names | Company-scoped CRUD API and list-details page | `InventJournalName` | Basic type setup exists; numbering and usage restrictions require SQL Server/runtime verification | Retain core maintenance and verify configured sequence dependencies | Critical | [Microsoft inventory journals](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals) |
| Journal header/lines | Standard persistence models, company-scoped API, and journal entry page | `InventJournalTable`, `InventJournalTrans` | Core UI/API is present; movement offset account now uses a company-scoped searchable `DimensionAttributeValueCombination` lookup, but other relational and per-type validations remain incomplete | Preserve posted-record restrictions and verify all journal types against SQL Server | Critical | [Microsoft inventory journals](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals); repository account lookup test |
| Transaction foundation | `InventDim`, `InventTransOrigin`, `InventTrans`, `InventSum`, journal posting service and focused tests | Same standard tables | SQLite lifecycle test covers opening stock through movement journal (including 1 box = 4 inventory pcs conversion), adjustment receipt/issue, transfer, movement, and counting; final on-hand reconciles to net `InventTrans.Qty`, generated ledger lines balance, and a later invalid line rolls back earlier writes. SQL Server rollback, duplicate/concurrent posting, and FIFO valuation reconciliation remain unverified | Verify SQL Server atomicity/idempotency and validate costing/ledger integration before claiming operational readiness | Critical | [Microsoft inventory journals](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals); [initialize stock levels](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/tasks/initialize-stock-levels-warehouse); [physical and financial updates](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates) |
| On-hand inquiry | Company-scoped `InventSum` inquiry joined to item/dimensions | `InventSum` and On-hand list | Basic inquiry implemented; no location/tracking/date filters | Add only filters backed by stored dimensions; reconcile after every journal test | High | Microsoft On-hand list documentation |
| Posting setup | Posting profile page/API and journal posting account resolution | `InventPosting` | Limited basic posting paths consume configured rules; all type/profile combinations need relational verification | Verify each journal type and fail posting for missing applicable setup | Critical | [Microsoft inventory posting profiles](https://learn.microsoft.com/dynamics365/finance/general-ledger/inventory-posting-profiles) |
| Costing and inventory value | Item model-group choice, journal cost fields, transaction and summary values | Item model group, `InventTrans`, `InventSum`, inventory close/recalculation and inventory value inquiry | Basic journal amounts use running-average-style behavior; FIFO/weighted-average settlement, standard-cost revaluation, closing/recalculation, and a reconciled value report are not implemented/verified | Expose only calculations that are implemented; add costing workflows/reporting and reconcile to ledger before claiming parity | Critical | [Inventory close](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-close); [running average cost](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/running-average-cost-price) |
| Ledger integration | Journal posting creates ledger entries for tested basic paths | Inventory posting profiles and general-ledger entries | Not verified against SQL Server, all posting combinations, or period behavior | Complete relational verification; report unsupported combinations explicitly | Critical | [Microsoft inventory posting profiles](https://learn.microsoft.com/dynamics365/finance/general-ledger/inventory-posting-profiles) |
| Permissions | Generic Inventory.Transactions permissions reused across pages | D365 duties/privileges concept | Present but too coarse for setup versus posting | Add least-privilege setup, journal maintain, journal post and inquiry permissions | Medium | Repository permission registry |
| Advanced WHS/manufacturing/variants | Some persistence columns exist but basic pages hide them | WHS, production, product masters/variants | Outside scope | Preserve stored columns; don't add services or UI workflows | Deferred | Agreed scope |

### Point-by-point scope confirmation

| Requested point | Current status | Confirmation / remaining gap |
| --- | --- | --- |
| Shared simple products and company-released products | Implemented; core regression tests pass | `EcoResProduct`/`EcoResDistinctProduct` shared identity and `InventTable` company release exist. Item/Service choice is wired into both product-creation paths; unsupported types are rejected. |
| Product number, names, description, search name | Implemented | Stored in shared product/search fields and `EcoResProductTranslation`; company item has its released-item name fields. |
| Product status/lifecycle | Missing | No verified basic product status field or lifecycle restrictions are exposed by this implementation. |
| Product categories | Missing | `EcoResCategory` is used by inventory posting criteria only; no product category hierarchy/assignment lifecycle exists. |
| Product identifiers and barcodes | Partial | Company-scoped backend CRUD API and released-product dialog exist. Server validation covers item, unit, optional dimension, positive quantity, and duplicate item/unit/dimension association. Barcode setup and concurrent uniqueness remain outstanding. |
| Product variants (configuration, size, color, style) | Excluded by request | No variant creation or maintenance is in scope. |
| Inventory parameters and number sequences | Partial | Basic parameters UI/API exists; journal sequence-reference integration and migration application are not verified. |
| Units and unit conversions | Implemented; live database/API verification pending | Basic shared units and standard/product-specific conversions have CRUD, conversion behavior, and SQLite tests. |
| Item model groups/costing methods | Partial | Setup supports several model labels, but FIFO, weighted-average settlement, and standard-cost revaluation are not implemented as costing parity. Sales-line inventory demand now respects the selected group's stocked flag (when configured); purchase integration is absent. |
| Site, warehouse, basic location; storage/tracking groups | Implemented in code; SQL Server/live flows pending | Basic setup exists. WHS execution and optional batch/serial processes remain out of scope; tracking-group assignment is present. |
| Inventory status | Unverified / not implemented | `InventDim.InventStatusId` exists, but no verified standard status setup/lookup lifecycle is exposed for this core. |
| Posting profiles and ledger integration | Partial | Basic journal paths resolve `InventPosting` and create ledger lines; SQL Server, period handling, all account types, and reconciliation need verification. |
| Inventory costing, adjustment, value reports, reconciliation | Partial | Journal costs and on-hand values exist; valuation support is limited and no verified cost close/recalculation or reconciled inventory value report exists. |

### Phase classification

- Working and aligned: company context for reviewed services, shared/simple products, product release, sites, warehouses, basic group setup, parameters, and basic inquiries.
- Present but incomplete: UOM conversion execution tests, released-product restriction coverage, dimension enforcement, transaction foundation, posting profiles, costing labels, and permissions.
- Present but incorrect if treated as operational: journal entity classes and costing/closing entity classes. Persistence alone is not a working D365 process.
- Missing and required: SQL Server application/verification of the basic `WMSLocation` migration, SQL Server transaction/rollback verification, concurrency/idempotency coverage, and full reconciliation.
- Outside scope: WHS execution, production/BOM/routes, planning, transportation, retail, variants, and advanced quality features.

### Implementation order established by Phase 1

1. Finish foundations: apply parameters migration, complete conversion maintenance, validate number sequences and permissions.
2. Complete basic configuration: `WMSLocation`, journal names, and used-record guards.
3. Harden product readiness and dimension resolution for the selected company's setup.
4. Implement and verify atomic journal posting for opening stock through movement, adjustment, transfer, and counting.
5. Reconcile `InventTrans`, `InventSum`, costs and any generated ledger entries after each journal type.
6. Run authenticated API/UI and concurrency tests in a dedicated database; SQLite unit tests alone are insufficient evidence.

## Core form fields

| Form | Core fields | Requirement / validation | Database mapping |
| --- | --- | --- | --- |
| Units of measure | Symbol, class, system, decimal precision | Symbol required and unique; precision 0-6; symbol locked after released-product use | `UnitOfMeasure` |
| Unit conversions | From unit, to unit, product scope, factor, fraction, offsets, rounding | Units must differ; factor positive; product optional; duplicate scope/pair rejected | `UnitOfMeasureConversion` |
| Sites | Site, name | Company-specific; code immutable after creation; cannot delete with warehouses | `InventSite` |
| Warehouses | Warehouse, name, site, standard type | Company-specific; selected site must exist in same company | `InventLocation` |
| Item model groups | Group ID, name, stocked product, inventory model, cost/physical-value policies, physical/financial posting, negative inventory, and basic physical update policies | Core inventory fields only; PDS/manufacturing, quarantine, and advanced reservation controls are not rendered. Existing API values remain in the record mapping and are preserved. | `InventModelGroup` |
| Basic inventory locations | Warehouse, location ID, input location, check text, location type, sort code | Location belongs to current-company warehouse; immutable code; cannot delete when referenced by an inventory dimension | `WMSLocation` |
| Inventory journals | Journal name, journal type, item, date, site/warehouse/location, unit, quantity or count, cost, offset ledger dimension, transfer destination | Server validates company scope, item/configuration, dimensions, units, type-specific values and posted-state restrictions. Movement offset selection searches valid company ledger-dimension combinations; adjustment uses posting profiles instead of a manual offset. | `InventJournalName`, `InventJournalTable`, `InventJournalTrans`, `InventDim` |
| On-hand | Item, name, site, warehouse, physical/reserved/available/ordered quantities and values | Read-only and company-specific | `InventSum` + `InventDim` + `InventTable` |

## Dependency order

Legal entity -> number sequences/parameters -> UOM -> sites/warehouses/locations -> item/model/dimension groups -> shared product -> company release -> units/prices/tax/default order settings -> posting setup -> journal names -> inventory journals -> on-hand/transactions/cost reconciliation.

## Verification

`Tests/InventoryArchitectureTests.cs` covers warehouse CRUD guards and company isolation, transaction inquiry costs and labels, on-hand standard buckets, and UOM duplicate/conversion/use restrictions. `Tests/InventSiteTests.cs` covers existing site behavior. These SQLite tests do not replace authenticated API checks against the deployed database.

## Persistent implementation checklist

This checklist is the delivery ledger for the basic D365 F&O-aligned inventory core. A phase is marked **Verified** only when its focused automated checks and build pass; database migration and authenticated browser/API checks remain explicitly separate evidence.

| Phase | Scope | Status | Evidence / blocker |
| --- | --- | --- | --- |
| 1 | Baseline mapping and gap matrix | Verified | Matrix, dependency order, scope boundaries, and standard-object classifications are recorded above. |
| 2 | Inventory parameters and number-sequence references | Implemented | Backend/API/UI and migration exist. Applying the migration and shared number-sequence runtime integration are deferred to the final Infrastructure checkpoint by request. |
| 3 | Units of measure and conversions | Verified | CRUD, duplicate/use restrictions, standard and product-specific conversion precedence, reciprocal conversion, focused tests, Finance build, and frontend typecheck passed. |
| 4 | Sites, warehouses, and basic locations | In progress | Site/warehouse tests pass; basic `WMSLocation` API/page and service guards are added. Entity is registered and migration generated; database application/runtime verification remains outstanding. No WHS execution features are included. |
| 5 | Item/model/storage/tracking groups and simple product readiness | In progress | Core lifecycle and assignments exist. Item/Service is now selectable and validated in shared/released product creation; five validator cases pass. Non-stocked Service readiness and sales-demand behavior still need implementation and integration tests. |
| 6 | Inventory journal names | Implemented | Company-scoped CRUD, type restrictions, lookups, UI, and focused service test exist. Infrastructure migration/runtime sequence verification remains final-checkpoint work. |
| 7 | Journal header/line lifecycle | In progress | Company-scoped API and basic journal entry UI support header/line maintenance, delete, and post. Movement offset uses a searchable company-scoped ledger-dimension lookup; SQL Server and authenticated browser flows remain unverified. |
| 8 | Atomic journal posting and on-hand reconciliation | In progress | SQLite lifecycle test covers opening stock, unit conversion, adjustment receipt/issue, transfer, movement, and counting; verifies on-hand equals net transaction quantity and a later invalid line rolls back earlier quantity/ledger writes. SQL Server rollback and duplicate/concurrent posting remain unverified. |
| 9 | Posting-profile and ledger validation | In progress | Basic posting paths consume `InventPosting`, create balanced ledger lines, and reject unconfigured account combinations. SQL Server/period behavior and FIFO settlement/close are not implemented or verified. |
| 10 | On-hand and transaction inquiries | In progress | Company-scoped inquiries exist; full posted-journal reconciliation across all supported types remains outstanding. |
| 11 | Permissions, used-record restrictions, and cross-company/concurrency tests | In progress | Current permissions are broad and the complete transaction/concurrency suite is pending. |
| 12 | Infrastructure completion and end-to-end acceptance | In progress | `WMSLocation` DbSet/model index and migration are added; `dotnet ef migrations has-pending-model-changes` reports no changes. The migration has not been applied; SQL Server, authenticated API/UI, posting, reconciliation, and browser acceptance remain outstanding. |

### Latest verification evidence (2026-10-21)

- `dotnet test IXApi/Tests/IXApi.Tests.csproj --no-restore -m:1 -v:q`: **264 passed, 0 failed, 2 skipped**.
- `dotnet test IXApi/Tests/IXApi.Tests.csproj --no-restore -m:1 -v:q --filter FullyQualifiedName~EcoResProductInputValidatorTests`: **5 passed, 0 failed, 0 skipped** (Item/Service accepted; BOM/product-master/unknown rejected).
- `dotnet test IXApi/Tests/IXApi.Tests.csproj --no-restore -m:1 -v:q --filter FullyQualifiedName~Non_stocked_model_group_does_not_create_sales_inventory_demand`: **1 passed, 0 failed, 0 skipped**.
- `npm run typecheck` and `npm run build` in `IXApp`: **passed**. Vite emitted the existing large-chunk advisory.
- `dotnet ef migrations has-pending-model-changes` for `ApplicationDbContext`: **no pending model changes** after adding `WMSLocation`.
- `git diff --check`: **passed** (line-ending notices only).
- SQL Server migration application, authenticated API/browser flows, concurrency, rollback, and full quantity/value/ledger reconciliation remain untested. The build emitted existing ImageSharp dependency vulnerability advisories; this inventory work did not change that dependency.

Current exclusions remain Advanced WHS, manufacturing/production, BOM/routes, planning, transportation, Commerce/retail, and product variants. Optional basic batch/serial behavior is conditional on the selected standard tracking dimension group; it is not replaced with a custom flag or entity.
