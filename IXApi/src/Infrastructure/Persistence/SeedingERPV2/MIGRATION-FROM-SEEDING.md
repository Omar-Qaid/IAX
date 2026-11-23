# Migration from `Seeding` to SeedingERPV2

SeedingERPV2 is the only seeding implementation. The legacy `Persistence/Seeding` folder has been removed. This file
records how its data and behaviour were carried over, and the deliberate differences.

## Profiles

| Profile | Contract | Runner | When |
| --- | --- | --- | --- |
| Baseline | `IBaselineSeeder` | `BaselineSeedRunner` | Application startup (`InitializeDatabaseAsync`); refused in Production |
| Examples | `IDataSeeder` | `SeedRunner` and the cycle examples | Explicit runs and tests only |

Baseline seeders are registered only as `IBaselineSeeder` (`AddBaselineSeeding`), so an example run never picks them
up and startup never runs an example. Installation-wide V2 seeders that both profiles need (settings, number
sequences, background jobs, notification templates, document types, workflow catalog) are reused in the baseline
through `BaselineSeederAdapter`; they are not duplicated.

Each baseline seeder runs in its own transaction together with its `SysDataSeedLog` row. Seeders only add what is
missing (`SeedContext.AddMissingAsync`); an existing row is left as the operator keeps it. The two one-time imports
(`Finance.Foundation.LegacyEmployees`, `Workflow.LegacyMaster`) run once per version and scope, recognise the history
rows the previous seeding wrote, and stay opt-in behind `Seeding:EnableLegacyImports` (embedded data additionally
needs `Seeding:EnableEmbeddedFixtures`, otherwise `ConnectionStrings:SeedDbConnString` is read).

## Where the data lives

| Module | Manifest | Seeders |
| --- | --- | --- |
| Foundation | `baseline-foundation.json`, `baseline-organization.json`, `legacy-employees.json` | companies, currencies and rates, fiscal calendar, dimensions, logistics, organization structure, employee import |
| General ledger | `baseline-generalledger.json` | ledger, chart of accounts, account structures |
| Tax | `baseline-tax.json` | authorities, periods, codes, groups, rates |
| Inventory | `baseline-inventory.json` | units, item groups, sites and warehouses, setup and posting, one released product |
| Accounts receivable | `baseline-accountsreceivable.json` | customer setup, payment modes, delivery and payment terms, charges, trade agreements |
| Administration | `baseline-administration.json` | disabled batch-job examples |
| Identity | `baseline-identity.json` | roles, users, permission catalog, company access |
| Workflow | `baseline-workflow.json`, `legacy-workflow-master.json` | process definitions, performers, print templates, workflow import |

`baseline-workflow.json` was exported from the rows the legacy seeding produced on an empty database. Rows with fixed
ids (processes 590, 603 and 651 and their example requests, performers) are added by id. Rows that were created with
database-assigned ids (the payment request, the request forms, their performers and print templates) are matched on
their natural key; a missing one is added with a new id and every reference to it, including the request-control ids
inside print layouts, is rewritten.

Seeder ids are the legacy ids, so existing history stays meaningful.

## Verified

- On SQL Server, the legacy seeding and the baseline were each run on an empty database and compared table by table:
  every legacy row is present in the baseline, except the deliberate differences below. A second baseline run added
  no rows.
- `BaselineSeedingTests`, `BaselineSeederBehaviorTests`, `LegacyImportSourceTests`, `PaymentRequestPrintTemplateTests`.
- `dotnet ef migrations has-pending-model-changes` reports no change: `SysDataSeedLog` moved namespace only.

## Deliberate differences

- **Tax authorities need their vendor.** `TaxAuthorityAddress.AccountNum` is a required foreign key to `VendTable`. The
  legacy seeder blanked it when the vendor was missing, which the key rejects, so on an empty database the legacy Tax
  seeder failed and the inventory setup that depends on it never ran. The baseline adds an authority, and its
  settlement periods, only once the vendor exists; tax codes, groups and rates are always seeded.
- **Account structures are assigned to the ledger on the first run**, not only on the second.
- **One-off repairs of specific historical rows were not carried over**: the DAT address and phone correction, the
  placeholder item-group fill, the `STANDARD` tax posting group rename, the Arabic product alias fix, the `FIFO` model
  group `MandatoryReceive` reset, the number-sequence counter catch-up for documents created under daily-cyclic
  formats, and the removal of two-part permission keys. Each targeted rows from an earlier seeder version and has
  already run on every database seeded before.
- **V2 corrections kept**: notification template wording (including the `{{ ame}}` typo), background job descriptions,
  the names of performers 13 to 16, and explicit `0` instead of `NULL` for unset number-sequence flags.
- **Users**: a new user's password comes from `Seeding:Users:{UserName}:Password`; without it the previous development
  password is used and a warning is logged.
- `wfData/wf.sql` and the unregistered `Chunks` seeders were not referenced and were removed without replacement.
