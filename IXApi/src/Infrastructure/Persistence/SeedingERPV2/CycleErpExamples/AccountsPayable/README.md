# Accounts Payable examples: configuration and master data only

This package prepares the existing module-owned demo configuration/master data and returns
ordered preparation steps plus persisted record references. It does not create purchase orders,
confirmations, receipts, invoices, inventory movements, vendor balances, payments, settlements,
bank movements or posted ledger entries. No transaction scenario is registered or executed.

## Implemented preparation

1. Validate the embedded example manifests and the existing company context.
2. Run the current 57 GL, Tax, Inventory, Cash/Bank and AP seeders in their actual dependency order.
3. Validate the stored configuration and master relationships inside the seed transaction.
4. Commit the masters/history atomically and return their references and completed seeder order.

The module seeders own all writes; this folder does not duplicate their setup or insert business
transactions. An existing legal entity is preserved; an absent selected company is created as demo master data. The existing APV2 conflict checks,
company scope, database seeder lock and safe rerun behavior apply.

## Invocation

```csharp
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;

services.AddAccountsPayableConfigurationAndMasterDataExamples();

// Use a fresh scope with the intended ApplicationDbContext company execution context.
using var scope = serviceProvider.CreateScope();
var examples = scope.ServiceProvider.GetRequiredService<AccountsPayableCycleSeeder>();
var report = await examples.PrepareAsync(new AccountsPayableExampleOptions(2026, actorUserId), ct);
```

Registration is opt-in and idempotent. No application startup hook executes it. The report's
`Scope` is `ConfigurationAndMasterData`; `TransactionsExecuted` is always `false`.
`PreparedAtUtc` is the report timestamp, not a document posting date.

## Data and ownership

- `Data/vendors.json`: the two named fictional suppliers and their setup keys.
- `Data/products.json`: stocked paper and nonstocked printer-maintenance master references.
- `Data/purchase-prices.json`: the three existing supplier quotes (SAR 25, SAR 350, USD 7).
- `Data/scenarios.json`: four configuration/master preparation steps, with transaction scenarios disabled.

These embedded files describe and verify the module-owned fixtures. They are not a second data
insertion mechanism. Edits that diverge from the owning seeders are rejected before writes;
stored values are checked again before commit. Database RecIds are resolved from company and
stable business keys, never copied from JSON.

`Reports/ScenarioExecutionReport.cs` records preparation only. Despite its historical filename,
`AccountsPayableReconciliationReport` contains master-data references, not posted reconciliation
balances. It includes ledger/chart/calendar, bank, warehouse/dimension, vendor/party/address/bank,
product/model-group and price references captured before commit. A report is returned only after
that transaction commits successfully.

See [the realistic demo](../../REAL-EXAMPLE.md) and [phase-one setup](../../PHASE-ONE.md).
Bank numbers and supplier identities are fictional demonstration data.

## Future reference only

The 20 folders under `Scenarios/` and transaction specifications in Documentation/06–12 are
retained for a future implementation phase. Their illustrative transaction amounts are not
runtime inputs or proof of posted results. The current implementation never invokes their
procedures. Posted-balance validator placeholders are intentionally unused in this scope.

## Verified results

- Build: 0 errors.
- Configuration/master tests: 11 passed within the API suite, including disposable SQL Server preparation and rerun.
- API suite: 801 passed, 2 skipped.
- Inventory suite: 97 passed.

The tests verify the four preparation steps, all 57 seeder IDs in execution order, persisted
vendor/product/price references, JSON report scope, stable IDs and row counts across reruns,
and zero rows in the 27 checked purchase/inventory/vendor/payment/tax/posting tables in a fresh
SQLite example database. SQL Server uses a generated disposable database and deletes it after
the first-run/rerun checks. These results validate preparation, not purchase-to-settlement posting.

The additional basic reference package seeds legal-entity fallback, storage/tracking groups and
stocked-item assignments, purchase/inventory order defaults, purchase type and purchase charge
code. Existing legal-entity data is preserved. See ../../PHASE-ONE.md for table names and scope.
`MasterData.AdditionalMasters` returns these persisted references. PurchParameters currently
stores the basic default; its transaction-phase consumption is not implemented here.
