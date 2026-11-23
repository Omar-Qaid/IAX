# Migrating `Seeding` into SeedingERPV2

`Persistence/Seeding` (about 112 C# files, two JSON imports, `wfData/wf.sql`) is the seeding that runs at
application startup. SeedingERPV2 is the replacement. This file records what the migration has to carry
across, how it fits V2's structure, and the order of work. `Seeding` is deleted only after every row below
is marked **Done** and the checks in the last section pass.

Decisions:

- **Two profiles.** *Baseline* replaces today's startup data and runs at startup. *Examples* is the existing
  V2 configuration, master data and cycle examples (APV2/ARV2 codes), which stay opt-in.
- **Legacy imports kept.** The OthersDB employee and workflow imports remain opt-in behind
  `Seeding:EnableLegacyImports`.
- **No schema change.** `SysDataSeedLog` keeps its table and columns. Only its CLR namespace moves.
- **Rewritten, not copied.** Every row is rewritten as a V2 `IDataSeeder`: data goes into a module catalog or a
  JSON manifest, writes go through `SeedContext.EnsureAsync`, and each module has an integrity validator.
  Where V2 already seeds the same table, the existing V2 seeder is reused with Baseline data instead of
  adding a second seeder.

## 1. Gaps in SeedingERPV2

### Core and execution

| Legacy | What it does | V2 today | Needed |
| --- | --- | --- | --- |
| `DatabaseSeederService` + `IDatabaseSeederService` | Startup entry point. Refuses Production. One transaction per seeder; failures are logged; `Required` failures stop startup. | `SeedRunner`: one serializable transaction, explicit call only. | A Baseline entry point in `Core/Execution` called from `ApplicationBuilderExtensions.InitializeDatabaseAsync`. |
| `SeedingOptions` (`Seeding` section) | `Enabled`, `EnableDemoData`, `EnableLegacyImports`, lock timeout, default company | none | Move to `Core/Models/SeedingOptions`, same section name and keys. |
| `SeederCategory`, `SeederExecutionPolicy`, `Version`, `Scope` | Eligibility (Demo, LegacyImport) and RunOnce | Every seeder is repeatable. | A profile/category on the Baseline registration. RunOnce only for the two imports. |
| `SqlServerSeederLock`, `ISeederLock` | Cross-instance app lock | V2 already **uses the legacy class**. | Move to `Core/Execution`. |
| `SysDataSeedLog` + configuration | Execution history | V2 already **uses the legacy entity**. | Move to `Core/Execution`. The key format `{id}:v{n}:{scope}` must still match, so existing databases don't re-run the RunOnce imports. |
| `IdentityInsertHelper` | Explicit identity keys | `SeedIdentityWriter` | Use `SeedIdentityWriter`; repoint `IdentityInsertHelperTests`. |
| `SeederDependencyGraph` | Topological order, missing/cyclic checks | `SeedDependencyResolver` | Covered. Repoint `SeedingDependencyGraphTests`. |
| `Chunks/*` | Old `ISeeder` implementations, not registered | — | Nothing to migrate. Only `OrganizationStructureTests` constructs one; repoint it. |

### Data (Baseline profile)

| Legacy seeder (Id) | Baseline data | V2 seeder to reuse or add |
| --- | --- | --- |
| `Finance.Foundation.CompanyInfo` | Parties and companies `dat` and `HBMC`; company claims for `sys` | Foundation `CompanyInfoSeeder` (company scoped); multi-company support plus a claims step in Identity |
| `Finance.Foundation.Logistics` | Countries and addresses | Foundation |
| `Finance.Foundation.Structure`, `Finance.Foundation.LegacyEmployees` | Organization hierarchy; OthersDB employee import (opt-in) | Foundation Hcm/Organization seeders |
| `Finance.Foundation.FinancialDimensions`, `Finance.Foundation.AccountStructures` | Dimensions, account structures | Foundation dimension and GL account-structure seeders |
| `Finance.GeneralLedger.Currencies` | SAR, USD, EUR, AED; 25 rate types; pairs and rates per type | Foundation `Currency*`/`ExchangeRate*` seeders |
| `Finance.GeneralLedger.FiscalCalendars` | Calendar `HBMC`, year 2026, periods 0–13 | Foundation `FiscalCalendarSeeder` |
| `Finance.GeneralLedger.Ledger` | Ledger with fiscal years open | GL `LedgerSeeder` |
| `Finance.GeneralLedger.MainAccounts` | The company chart (`10010000` …) | GL `MainAccountSeeder` and a chart manifest |
| `Finance.GeneralLedger.Tax` | Tax codes, groups and posting | Tax module |
| `Finance.Inventory.UnitOfMeasure` | 27 units, base units, conversions | Inventory `UnitOfMeasureSeeder` |
| `Finance.Inventory.ItemGroups`, `Finance.Inventory.Setup`, `Finance.Inventory.SitesAndWarehouses` | Item and model groups, posting, parameters, sites, warehouses | Inventory configuration and master data |
| `Finance.AccountsReceivable.*` (7) | Customer groups, ledger posting, payment modes, delivery setup, markup, price/discount, sales pools | AR configuration |
| `Administration.Settings`, `Administration.NumberSequences` | Settings, batch examples, number sequences | Administration configuration |
| `Identity.MasterData` | Roles, users `omar` and `sys`, permissions | **New** `Modules/Identity`. Passwords are currently hard-coded `"123"`; Baseline should read them from configuration. |
| `Organization.DocumentManagement` | Document types | Organization `DocuTypeSeeder` |
| `Workflow.ProcessMaster` | Workflow processes, forms, performers, payment request, processes 590/603/651 | Workflow module: catalog seeders exist; process definitions are **new** |
| `Workflow.LegacyMaster` | OthersDB or embedded-JSON workflow import (opt-in) | Workflow module, opt-in |

`wfData/wf.sql` is not referenced by any code or project item; it is not migrated.

### References to update

`Bootstrap/DependencyInjection.cs`, `Bootstrap/Extensions/ServiceCollectionExtensions.cs` (`AddSeedingCore`),
`Bootstrap/Extensions/ApplicationBuilderExtensions.cs`, `ApplicationDbContext`, `AuditInterceptor`,
`ErpModelBuilderExtensions`, the two embedded resources in `IAX.IXApi.Infrastructure.csproj`, `Tests/SeedExecutionContracts.json`,
and the tests that use legacy types: `IdentityInsertHelperTests`, `SeedingDependencyGraphTests`,
`SeedingExecutionTests`, `FiscalCalendarSeederTests`, `LedgerSeederTests`, `LogisticsSeederTests`,
`OrganizationStructureTests`, `LegacyWorkflowSeedDeserializationTests`, `PaymentRequestPrintTemplateTests`,
`AuthorizationPolicyTests`, `AccountsReceivableSeedIntegrityTests`, `AllModulesSeedIntegrityTests`.

## 2. Order of work

1. **Core.** Move the log entity, the lock and the options into V2 Core; add the Baseline profile and startup runner. **Pending**
2. **Finance baseline.** Company, currencies, calendar, ledger, chart, dimensions, tax, inventory and AR, reusing V2 seeders with Baseline manifests. **Pending**
3. **Administration, Organization, Identity.** **Pending**
4. **Workflow.** Process definitions, forms, performers, 590/603/651, and the opt-in import. **Pending**
5. **Switch.** Point startup and DI at V2. Port or replace the legacy tests. **Pending**
6. **Remove `Seeding`.** **Pending**

## 3. Checks before removal

- Baseline on an empty database produces the same rows (by business key) as legacy `Seeding` does.
- A second Baseline run adds no rows.
- An existing database seeded by legacy `Seeding` is accepted unchanged: no conflicts, and RunOnce imports are not repeated.
- `dotnet ef migrations has-pending-model-changes` reports no change.
- The Examples profile and all seed test suites still pass.
