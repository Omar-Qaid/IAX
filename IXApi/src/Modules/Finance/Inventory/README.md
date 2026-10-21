# Inventory backend architecture

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
| Shared product | Product API/pages and EF mappings | `EcoResProduct`, `EcoResDistinctProduct`, `EcoResProductTranslation`; Products V2 data entity is `EcoResProductV2Entity` | Working for simple distinct products; variants deliberately excluded | Regression-test shared identity and translations | High | Microsoft product data entities documentation |
| Released product | Release API/page and released-product details | `InventTable`; Released products V2 data entity is `EcoResReleasedProductV2Entity` | Working for basic release and company setup | Preserve `InventTable.Product -> EcoResProduct.RecId`; expand readiness tests | Critical | Microsoft product data entities documentation |
| Released-item setup | Units, commercial defaults, group assignments and default-order settings | `InventTableModule`, `InventItemGroupItem`, `InventModelGroupItem`, `EcoRes*DimensionGroupItem`, `InventItem*Setup` | Present and substantially implemented | Test all server validations and used-record restrictions | High | Repository service mappings and released-product tests |
| Inventory parameters | Company-scoped GET/PUT API and `SetupPage` UI | `InventParameters` and Inventory and warehouse management parameters form | Implemented basic surface; no number-sequence reference surface yet | Apply migration; connect verified inventory journal sequence references during journal phase | High | Microsoft `InventParameters` CDM metadata and number-sequence documentation |
| Units and conversions | Shared UOM and conversion CRUD with list-details maintenance UI | `UnitOfMeasure`, `UnitOfMeasureConversion` | Basic maintenance implemented; conversion execution rounding still requires transaction-level coverage | Add rounding execution tests when journal quantity conversion is integrated | High | Repository UOM tests and Microsoft unit conversion behavior |
| Sites and warehouses | CRUD pages/APIs with company validation | `InventSite`, `InventLocation` | Working and aligned for basic scope | Retain company and owning-site restrictions | High | Repository inventory architecture tests |
| Basic inventory locations | No standard implementation found | `WMSLocation` | Missing and required for location-enabled basic storage dimensions | Add basic `WMSLocation` table/API/page without WHS profiles, license plates, or mobile processes | High | Standard inventory dimension metadata |
| Item and model groups | Setup pages/APIs and assignment tables | `InventItemGroup`, `InventItemGroupItem`, `InventModelGroup`, `InventModelGroupItem` | Present; transactional use restrictions need broader coverage | Add referenced-record delete/update tests | High | Repository services and D365 released-product setup behavior |
| Storage/tracking groups | Setup and assignment pages/APIs | `EcoResStorageDimensionGroup*`, `EcoResTrackingDimensionGroup*` | Basic setup present; optional batch/serial behavior not yet transaction-tested | Enforce active/blank dimension rules in journal line validation | Critical | Microsoft inventory dimension documentation |
| Journal names | `InventJournalName` persistence model only | `InventJournalName` | Missing service, page, numbering validation, and used-record restrictions | Implement basic movement, adjustment, transfer and counting journal-name setup | Critical | Microsoft inventory journals documentation |
| Journal header/lines | `InventJournalTable`, `InventJournalTrans`, `InventCountJour` persistence models only | Same standard tables | Missing lifecycle and posting engine; current entities expose many deferred advanced fields | Build basic DTOs/services/forms around core fields; don't expose advanced fields | Critical | Microsoft inventory journals documentation |
| Transaction foundation | `InventDim`, `InventTransOrigin`, `InventTrans`, sales-demand integration and tests | Same standard tables | Present but not yet proven for journal posting, atomicity, idempotency or concurrency | Introduce journal-origin links and one transactional posting boundary | Critical | Microsoft inventory transaction and consistency-check documentation |
| On-hand inquiry | Company-scoped `InventSum` inquiry joined to item/dimensions | `InventSum` and On-hand list | Basic inquiry implemented; no location/tracking/date filters | Add only filters backed by stored dimensions; reconcile after every journal test | High | Microsoft On-hand list documentation |
| Posting setup | Posting profile page/API | `InventPosting` | Setup exists; journal posting doesn't consume it | Implement account resolution and fail posting when required setup is absent | Critical | Microsoft inventory posting documentation |
| Costing | Item model-group labels and transaction cost columns | Item model groups, inventory costing/close | Present but incomplete; no verified FIFO settlement/recalculation/close | Do not claim FIFO valuation until receipt/issue settlement is implemented and reconciled | Critical | Microsoft inventory costing documentation; repository has no close service |
| Ledger integration | Ledger dimension references exist | Inventory subledger vouchers and general ledger entries | Missing verified journal voucher creation and period validation | Integrate only after quantity posting is atomic; otherwise report quantity-only readiness | Critical | Microsoft inventory posting documentation |
| Permissions | Generic Inventory.Transactions permissions reused across pages | D365 duties/privileges concept | Present but too coarse for setup versus posting | Add least-privilege setup, journal maintain, journal post and inquiry permissions | Medium | Repository permission registry |
| Advanced WHS/manufacturing/variants | Some persistence columns exist but basic pages hide them | WHS, production, product masters/variants | Outside scope | Preserve stored columns; don't add services or UI workflows | Deferred | Agreed scope |

### Phase classification

- Working and aligned: company context for reviewed services, shared/simple products, product release, sites, warehouses, basic group setup, parameters, and basic inquiries.
- Present but incomplete: UOM conversion execution tests, released-product restriction coverage, dimension enforcement, transaction foundation, posting profiles, costing labels, and permissions.
- Present but incorrect if treated as operational: journal entity classes and costing/closing entity classes. Persistence alone is not a working D365 process.
- Missing and required: basic `WMSLocation`, journal-name maintenance, journal lifecycle/posting, posting-account consumption, atomic/idempotent posting tests, and reconciliation tests.
- Outside scope: WHS execution, production/BOM/routes, planning, transportation, retail, variants, and advanced quality features.

### Implementation order established by Phase 1

1. Finish foundations: apply parameters migration, complete conversion maintenance, validate number sequences and permissions.
2. Complete basic configuration: `WMSLocation`, journal names, and used-record guards.
3. Harden product readiness and dimension resolution for the selected company's setup.
4. Implement atomic journal posting in order: adjustment/opening, movement, transfer, then counting.
5. Reconcile `InventTrans`, `InventSum`, costs and any generated ledger entries after each journal type.
6. Run authenticated API/UI and concurrency tests in a dedicated database; SQLite unit tests alone are insufficient evidence.

## Core form fields

| Form | Core fields | Requirement / validation | Database mapping |
| --- | --- | --- | --- |
| Units of measure | Symbol, class, system, decimal precision | Symbol required and unique; precision 0-6; symbol locked after released-product use | `UnitOfMeasure` |
| Unit conversions | From unit, to unit, product scope, factor, fraction, offsets, rounding | Units must differ; factor positive; product optional; duplicate scope/pair rejected | `UnitOfMeasureConversion` |
| Sites | Site, name | Company-specific; code immutable after creation; cannot delete with warehouses | `InventSite` |
| Warehouses | Warehouse, name, site, standard type | Company-specific; selected site must exist in same company | `InventLocation` |
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
| 4 | Sites, warehouses, and basic locations | In progress | Site and warehouse behavior is covered. Standard `WMSLocation` basic maintenance is still missing; no WHS execution features will be added. |
| 5 | Item/model/storage/tracking groups and simple product readiness | In progress | Core lifecycle and assignments exist. Dimension-rule and used-record regression coverage is incomplete. |
| 6 | Inventory journal names | Implemented | Company-scoped CRUD, type restrictions, lookups, UI, and focused service test exist. Infrastructure migration/runtime sequence verification remains final-checkpoint work. |
| 7 | Journal header/line lifecycle | In progress | Standard persistence tables exist; operational DTOs, validation, API, and UI are not yet complete. |
| 8 | Atomic journal posting and on-hand reconciliation | Not started | Must post through `InventTransOrigin`, `InventTrans`, and `InventSum`, reject duplicate posting, and enforce negative-inventory/dimension/UOM rules. |
| 9 | Posting-profile and ledger validation | Not started | `InventPosting` setup exists, but journal posting does not yet resolve accounts or create verified accounting entries. No synthetic voucher behavior will be claimed. |
| 10 | On-hand and transaction inquiries | Implemented | Company-scoped inquiries exist; reconciliation against posted journals remains part of Phase 8 verification. |
| 11 | Permissions, used-record restrictions, and cross-company/concurrency tests | In progress | Current permissions are broad and the complete transaction/concurrency suite is pending. |
| 12 | Infrastructure completion and end-to-end acceptance | Blocked by sequence | Intentionally last: review/apply migrations and model mappings, then run SQL Server, authenticated API/UI, posting, reconciliation, and browser acceptance tests. |

Current exclusions remain Advanced WHS, manufacturing/production, BOM/routes, planning, transportation, Commerce/retail, and product variants. Optional basic batch/serial behavior is conditional on the selected standard tracking dimension group; it is not replaced with a custom flag or entity.
