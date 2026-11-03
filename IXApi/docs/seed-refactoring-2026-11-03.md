# ERP seed refactoring and integrity review

## Scope and preservation

This change refactors existing seed definitions only. It introduces no entities, migrations, accounting rules, demo setup values, or changes to the registered dataset. Existing seed IDs, versions, categories, dependencies, policies, company scopes and completion records are retained. No application database was seeded or modified during verification.

Active definitions formerly in `Chunks/` are organized under `Modules/Finance`, `Modules/Administration`, `Modules/Identity`, `Modules/Organization`, and `Modules/Workflow`. Finance registration now sits beside the other module registrations in `Registrations/`. Legacy CLR namespaces remain unchanged to preserve callers and partial-class relationships. Unregistered legacy alternatives remain unregistered; `Chunks/` retains those alternatives and shared legacy contracts/import support. The Infrastructure assembly continues to own persistence-dependent code, avoiding circular references with the business module assemblies.

`FinanceOperationalExamplesSeeder` is split into ledger/bank/subledger, return, and inventory definition files. It remains one repeatable demo seeder with the same identity and transaction, invocation order, business keys, data values, and save boundaries. Posting examples reuse a small Finance helper for their existing company-scoped SQL Server context construction.

## Execution integrity

- The existing shared `IModuleSeeder`, DI registrations, deterministic dependency ordering, execution history, logging, timing, SQL Server application lock and run-once uniqueness protection are reused.
- Missing dependencies and cycles are now validated across all registrations before filtering optional categories. Broken disabled branches can no longer remain hidden until their first enabled deployment.
- Cancellation rolls back the current relational transaction and propagates as cancellation, leaving neither partial rows nor a misleading failure/completion record. A later run remains eligible.
- The execution-contract inventory in `Tests/SeedExecutionContracts.json` captures all 51 registered seeders and the existing unregistered purchase-settlement seeder. Focused tests check registered type, ID, version, category, policy, scope and dependencies against that inventory, including both opt-in branches.

## Existing limitations retained

`SalesScenarioSeeder`, `SalesCompletionScenarioSeeder`, `CommercialScenarioSeeder`, `SalesAdvancedScenarioSeeder`, and `InventoryFullCycleSeeder` use separate contexts because confirmation, delivery, pricing and other called services begin transactions directly. Their business data commits through those services before the outer completion-history transaction. Existing stable business/request keys support resumption, but these examples do **not** provide atomic business-data-plus-success-history commits. Their service transaction boundaries and concurrency locks are preserved; changing posting services is outside this seed-only refactor. The unregistered `PurchaseSettlementSeeder` likewise retains its independent service context and remains excluded.

Other relational seeders using `SeedingContext.Db` retain atomic data/history commits through the orchestrator. SQLite coordination remains process-local; multi-instance SQL Server concurrency is not claimed from SQLite checks.

No historical data conflicts were searched for in a live database. Existing compiler/package warnings are outside this refactor.

## Table coverage

The current model snapshot contains 275 distinct table names, including 35 absent from the October coverage inventory. See `seed-table-coverage-review-2026-11-03.csv` for every table. Historical counts are explicitly historical, not current verification. The model-only coverage test compares this complete inventory with the current EF model so future table additions require review.

The older inventory's runtime logs, caches, authentication credentials/identities, optional role claims and retired archive exclusions remain explicit. Existing posting services generate transaction tables; they must not be filled with disconnected fabricated records. This refactor makes no claim that every business table is populated on a fresh SQL Server database. Completing new table datasets would require a separate seed-data expansion, which the final refactoring scope excludes.

## Focused verification

Run the seed tests against isolated SQLite databases and an isolated build output path:

```powershell
dotnet test IXApi/Tests/IXApi.Tests.csproj --artifacts-path .artifacts/seed-refactor-check -p:NuGetAudit=false -m:1 -nr:false --filter "FullyQualifiedName~Seed"
dotnet build IXApi/IXApi.csproj --no-restore -m:1 -nr:false
```

The tests cover graph validation, metadata preservation, run-once history, version/scope isolation, rollback/retry, cancellation, reruns, company/customization preservation, and mapped-table inventory. SQL Server posting scenarios and production seeding are not executed by these checks.

Verified results: API build passed with zero errors; all 44 focused seed tests passed. The 35 moved definition files match their original contents, and all 104 string literals in the split operational seeder are preserved. The initial sandboxed testhost timed out; the successful test run used the same isolated outputs outside the sandbox. Evidence: `.artifacts/seed-refactor-final-build.log` and `.artifacts/seed-refactor-final-tests.log` at the repository root. No live application database was accessed.
