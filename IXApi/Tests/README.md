# IXApi tests

`IXApi.Tests.csproj` is the test project: it compiles every `.cs` file under this folder. Run it with:

```powershell
dotnet test IXApi/Tests/IXApi.Tests.csproj
dotnet test IXApi/Tests/IXApi.Tests.csproj --filter "FullyQualifiedName~LedgerJournal"   # one area
```

## Layout

Tests are grouped by the module they cover, then by the kind of test:

```
Tests/
  Support/                  shared test infrastructure (no tests)
    Identity/               TestIdentity: acting user and company
    Persistence/            SQLite and SQL Server test databases
    Builders/               reusable data builders
    TestEnvironment.cs      opt-in switches and the [SqlServer*Fact] attributes
  Resources/                embedded baselines read by tests
  Architecture/  Api/  Security/
  Administration/  Communication/  Organization/  Workflow/
  Finance/
    Architecture/  Acceptance/        cross-module Finance rules and the full example cycle
    Common/  Settlement/              shared posting infrastructure, customer and vendor settlement
    Foundation/  GeneralLedger/  AccountsPayable/  AccountsReceivable/  Inventory/  Tax/
  Seeding/
    Baseline/               the startup baseline profile
    Examples/               the example (cycle) profile and its integrity checks
```

Inside a module the test type is a subfolder:

| Folder | What the tests do |
| --- | --- |
| `Unit` | No database: logic, validators, mappings, model metadata. |
| `Integration` | Run services against an in-memory SQLite database (some also against opt-in SQL Server). |
| `Architecture` | Structural rules over assemblies, controllers, enums and table ids. |
| `Acceptance` | Multi-stage business cycles from setup to posting, close and reconciliation. |

Large test classes are `partial` and split by topic into `<Class>.<Topic>.cs` files next to the main file, which keeps
the shared helpers and fixtures (for example `InventoryCycleTests.Close.cs`, `InventoryCycleTests.Sales.cs`,
`LedgerJournalPostingTests.Reversal.cs`). Add a new test to the topic file it belongs to.

All test files use the single namespace `IAX.IXApi.Tests`. Namespaces deliberately do not follow folders: a
namespace such as `IAX.IXApi.Tests.Workflow` would shadow the production `Workflow.*` names inside test code.

## Writing a test

- **Database.** For the full application model use `SqliteTestDatabase`:

  ```csharp
  await using var database = await SqliteTestDatabase.CreateAsync();   // schema created, foreign keys on
  var db = database.Context;
  ```

  It creates the schema from the production model script, so keys, indexes and relationships behave as on SQL Server.
  Pass `createSchema: false` when the test times schema creation as a stage, then call `database.CreateSchemaAsync`.
  `database.CreateContext()` gives a second context on the same database. Tests that only need a handful of entities
  may keep a small private `DbContext` of their own; that keeps them fast and independent of unrelated mappings.
- **Acting user.** Use `new TestIdentity("my-test")`. It is always authorized and works in company `dat`; set
  `DataAreaId` to switch company. The user id lands in `CreatedBy`/`OwnerAccountId`.
- **SQL Server.** Mark the test `[SqlServerFact]` and create the database with
  `await using var database = SqlServerTestDatabase.Create("IXApi_MyArea_")`. The name always gets a new GUID and the
  database is dropped on dispose, also when the test fails.
- **Isolation.** Each test creates its own database; nothing is shared between tests. Tests that change global state
  (for example Mapster's global configuration) only add registrations, never remove them.

## Opt-in SQL Server runs

| Variable | Enables |
| --- | --- |
| `IXAPI_AP_PHASE_ONE_SQLSERVER=<server>` | `[SqlServerFact]` tests on disposable databases on that server |
| `IAX_SEED_ERP_V2_SQL_TESTS=1` | `[SqlServerSeedFact]` all-module seed replay |
| `IAX_BATCH_SQL_TESTS=1` | `[SqlServerBatchFact]` batch session-lock tests |

`Run-FullExampleValidation.ps1` sets all three and requires every test to pass with no skips.

## Focused projects

`InventoryCycle.Tests`, `SalesOrderConfirmation.Tests`, `CustomerMapping.Tests`, `CustomerPostingProfiles.Tests` and
`TaxAccountGroup.Tests` compile one or two of the files above against only the Finance module, for a faster inner
loop. Their tests also run in `IXApi.Tests`. Files they compile must not use `Support/`, which needs the
infrastructure project.
