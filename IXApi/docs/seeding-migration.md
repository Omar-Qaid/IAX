# Database seeding migration and inventory

## Execution contract

`DatabaseInitialization:Enabled` enables migration and seeding outside Production. `Seeding:EnableDemoData`, `Seeding:EnableLegacyImports`, and `Seeding:EnableEmbeddedFixtures` default to false. A legacy import additionally needs `ConnectionStrings:SeedDbConnString` or explicit embedded-fixture permission. The registered graph is checked before any seeder runs. Each eligible seeder runs in dependency order with an alphabetical tie-breaker.

Repeatable seeders run on every eligible startup and must use their own natural keys. Run-once seeders run once per ID, version, and scope. Their successful data and history row commit in one relational transaction. Failed attempts roll back, then write a failure row; dependent seeders are skipped. Independent seeders continue, but startup reports an aggregate failure. A dedicated SQL Server session holds `sp_getapplock` across the entire run. A filtered unique index also prevents two successful run-once rows for the same ID, version, and scope. SQLite and InMemory use only process-local coordination, so they are test providers rather than supported multi-instance startup targets.

`SysDataSeedLogs.TableName` remains for existing rows. New rows also store seeder ID, version, scope, policy, outcome, duration, completion time (`CreatedAt` in UTC), and error. Old rows with no `SeederId` are recognized by the old name or composite key. The migration leaves old rows intact. After a failure, correct the cause and restart; only a successful row prevents a run-once retry. Increase `Version` only when the intended data contract changes.

The SQL Server identity-insert helper validates table identifiers and attempts `OFF` in `finally` even after cancellation. It uses the caller's current transaction when one exists. Some older workflow process files still use direct `IDENTITY_INSERT`; they retain their existing table-specific save order and need separate conversion and SQL Server integration coverage before replacement.

## Seeder inventory

Paths below are relative to `src/Infrastructure/Persistence/Seeding`. `C/` means `Chunks/`; `F/` means `Modules/Finance/`. All listed seeders are version 1. The source file is the authoritative list of exact text values and fixed keys; this table names the owned data and calls out explicit `RecId` literals. `dat` is a fixed company scope in the existing data, even where a metadata scope is `global`.

| Old seeder / current source | Stable ID or decision | Data and fixed-key source | Category / policy / scope | Dependencies / registration |
| --- | --- | --- | --- | --- |
| `C/SysSettingsSeeder.cs` | `Administration.Settings` | Settings, notification templates, batch examples | Required / repeatable / global | Registered; no dependency |
| `C/NumberSequenceSeeder.cs` | `Administration.NumberSequences` | Number sequences and document references | Required / repeatable / global | Registered; no dependency |
| `C/DocumentManagementSeeder.cs` | `Organization.DocumentManagement` | Document types | Required / repeatable / global | Registered; no dependency |
| `C/IdentitySeeder.cs` | `Identity.MasterData` | Users, roles, permissions and assignments | Required / repeatable / global | Settings; registered |
| `C/MainAccountSeeder.cs` | `Finance.GeneralLedger.MainAccounts` | Main accounts | Reference / repeatable / dat | CompanyInfo; registered |
| `C/CustPaymModeSeeder.cs` | `Finance.AccountsReceivable.PaymModes` | Customer payment modes | Reference / repeatable / dat | CompanyInfo; registered |
| `C/SalesPoolSeeder.cs` | `Finance.AccountsReceivable.SalesPools` | Sales pools | Reference / repeatable / dat | CompanyInfo; registered |
| `C/MarkupTableSeeder.cs` | `Finance.AccountsReceivable.Markup` | Charge and markup tables | Reference / repeatable / dat | CompanyInfo; registered |
| `C/ErpSeeder.cs` | Split; old class excluded | Company, logistics, ledger, units, customers, products and setup | Legacy class; no direct run | Split into the ten `F/` classes below to avoid running the old mixed dataset twice |
| `F/CompanyInfoSeeder.cs` | `Finance.Foundation.CompanyInfo` | `CompanyInfo`, party roots; DAT and HBMC | Required / repeatable / dat | Registered; no dependency |
| `F/LogisticsAddressSeeder.cs` | `Finance.Foundation.Logistics` | Geographic and postal address reference data | Reference / repeatable / dat | CompanyInfo; registered |
| `F/FiscalCalendarSeeder.cs` | `Finance.GeneralLedger.FiscalCalendars` | Calendars, periods, ledger periods | Reference / repeatable / dat | CompanyInfo; registered |
| `F/CurrencyAndExchangeRateSeeder.cs` | `Finance.GeneralLedger.Currencies` | Currencies, exchange rate types, pairs and rates | Reference / repeatable / dat | CompanyInfo; registered |
| `F/UnitOfMeasureSeeder.cs` | `Finance.Inventory.UnitOfMeasure` | Units, base units and conversions | Reference / repeatable / global | CompanyInfo; registered |
| `F/InventItemGroupSeeder.cs` | `Finance.Inventory.ItemGroups` | Inventory item groups | Reference / repeatable / global | CompanyInfo; registered |
| `F/CustGroupSeeder.cs` | `Finance.AccountsReceivable.CustGroups` | Customer groups | Reference / repeatable / global | CompanyInfo; registered |
| `F/SalesDeliverySetupSeeder.cs` | `Finance.AccountsReceivable.DeliverySetup` | Delivery and payment terms and schedules | Reference / repeatable / dat | CompanyInfo; registered |
| `C/PriceDiscSeeder.cs` | `Finance.AccountsReceivable.PriceDisc` | Price and discount groups and agreements | Reference / repeatable / dat | CustGroups; registered |
| `C/InventorySiteWarehouseSeeder.cs` | `Finance.Inventory.SitesAndWarehouses` | Sites, warehouses and WMS locations | Reference / repeatable / dat | CompanyInfo; registered |
| `C/VendSeeder.cs` | `Finance.AccountsPayable.Vendors` | Vendor groups, vendors, parties | Reference / repeatable / dat | CompanyInfo; registered |
| `C/TaxSeeder.cs` | `Finance.GeneralLedger.Tax` | Tax codes, groups, authorities, periods and posting | Reference / repeatable / dat | CompanyInfo; registered |
| `C/InventorySeeder.cs` | `Finance.Inventory.Setup` | Groups, shared and released product, dimensions and posting rules | Reference / repeatable / dat | UnitOfMeasure; registered |
| `C/CustLedgerSeeder.cs` | `Finance.AccountsReceivable.CustLedger` | Customer ledger accounts and setup | Reference / repeatable / dat | CompanyInfo; registered |
| `C/OthersDBOrganizationEmployeeSeeder.cs` | `Finance.Foundation.LegacyEmployees` | Imported departments, occupations, genders, nationalities, parties and workers; source IDs retained | Legacy import / run-once / dat | CompanyInfo; registered, opt in |
| `C/OrganizationStructureSeeder.cs` | `Finance.Foundation.Structure` | Organization units, roles, hierarchy, positions and assignments | Reference / repeatable / dat | CompanyInfo; registered |
| `C/HcmExampleSeeder.cs` | `Finance.Foundation.HcmExamples` | Example workers and assignments | Demo / repeatable / dat | Structure; registered, opt in |
| `C/OthersDBWorkflowMasterFromSeeder.cs` | `Workflow.LegacyMaster` | Imported workflow lookup and process graph; literal `RecId` 1–5 plus source IDs | Legacy import / run-once / global | Identity; registered, opt in |
| `C/WfProcessSeedData.cs` and `Processes/Process590`, `Process603`, `Process651` | `Workflow.ProcessMaster` | Workflow masters, forms, templates and fixed process IDs 590, 603, 651 | Required / repeatable / global | Identity; registered |
| `C/OthersDBWorkflowRequestFromSeeder.cs` | `Workflow.DemoRequestTracking` | Example requests, assignments, reports and history | Demo / repeatable / dat | ProcessMaster; registered, opt in |
| `C/OrganizationSeeder.cs` (`OrganizationDemoSeeder`) | `Finance.Foundation.OrgDemo` | Synthetic organization data; literal `RecId` 1–5 among others | Demo / repeatable / dat | Structure; registered, opt in |
| `F/SampleCustomerVendorSeeder.cs` | `Finance.AccountsReceivable.DemoCustVend` | Sample customers, vendors and parties | Demo / repeatable / dat | CustGroups, Logistics; registered, opt in |
| `F/SampleProductSeeder.cs` | `Finance.Inventory.DemoProducts` | Sample products, units and packaging | Demo / repeatable / dat | ItemGroups, UnitOfMeasure; registered, opt in |
| `C/DimensionSeeder.cs` | Intentionally excluded | Old dimension setup | Legacy `ISeeder`; no direct run | Unregistered in the old pipeline; preserve exclusion |
| `C/OrganizationUnitSeeder.cs` | Intentionally excluded | Organization units | Legacy `ISeeder`; no direct run | Replaced by Structure seeder; preserve exclusion |
| `C/PostingProfileSeeder.cs` | Intentionally excluded | Posting profiles | Legacy `ISeeder`; no direct run | Unregistered in the old pipeline; preserve exclusion |
| `C/WorkflowSeeder.cs` | Intentionally excluded | Workflow graph; literal IDs include 0–23 and 591–595 | Legacy `ISeeder`; no direct run | Replaced by ProcessMaster; preserve exclusion |
| `C/WfValidateSeedData.cs` | Intentionally excluded | Workflow validation and repair | Legacy `ISeeder`; no direct run | Manual repair utility; preserve exclusion |
| `C/OrganizationSeeder.cs` (`OrganizationSeeder`) | Intentionally excluded | Wrapper for legacy employee import | Legacy `ISeeder`; no direct run | Direct legacy-import registration owns this path |

`C/OthersDBSeedData.cs` is an abstract import helper, not a runnable seeder. `Data/*.json` and `wfData/wf.sql` are fixture/source files, not independent registrations. Partial files under `Processes/` execute through `WfProcessSeedData` and are not separate graph nodes. Many fixed IDs are derived from embedded JSON or workflow arrays; their exact values are in those source files and are not rewritten by this migration.

## Add a module seeder

1. Add a focused class deriving from `BaseModuleSeeder` in the owning module's seeding area. Give it a stable ID, version, category, scope, policy, and explicit dependencies.
2. Implement `SeedAsync(SeedingContext, CancellationToken)` for new code. Query by the entity's real business key, including company, and use `IgnoreQueryFilters()` only with an explicit company predicate.
3. Register it as `IModuleSeeder` in the module registration method called by `AddSeedingCore`. Avoid constructing it in `DatabaseSeederService`.
4. Add a rerun test and a dependency-graph test. For run-once data, verify success history, failure rollback, and retry. For explicit identity values, use `IIdentityInsertService` and verify cleanup on SQL Server.
5. Document the exact data and IDs here. Increase the version only after deciding how existing installations should be updated.

## Current limitations and verification

Seeder classes remain in Infrastructure because many use `ApplicationDbContext` and Identity managers directly; moving them into the Finance or Workflow assemblies would introduce a project reference cycle. Data ownership is expressed through registration and stable IDs until those dependencies are replaced with module interfaces.

No equivalent before/after SQL Server databases or source import database were supplied for a row-by-row comparison. Before enabling initialization for an existing database, compare row counts, keys, references, and company scope in a restored copy and test legacy imports separately. The Production initialization guard remains in `Program.cs`.
