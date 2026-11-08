# Accounts Payable examples

This package has two explicit entry points. `AccountsPayableCycleSeeder.PrepareAsync` prepares configuration and master data only. `AccountsPayableTransactionExamples.RunAsync` prepares those prerequisites and executes the selected transaction examples through the owning business services. Neither entry point runs at application startup.

See [phase two](PHASE-TWO.md) for the opt-in transaction API, implemented scenarios and boundaries.

## Implemented preparation

1. Validate the embedded example manifests and the existing company context.
2. Run the current 58 GL, Tax, Inventory, Cash/Bank and AP seeders in their actual dependency order.
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

## Scenario documentation

The transaction runner currently implements IDs 01-09, 11-16, 18-20 and 21. IDs 10 (PO prepayment invoice) and 17 (receipt correction) remain outside the executable catalog. Older scenario specifications describe intended behavior; the executable catalog and phase-two guide identify the current implementation. Phase-one manifests remain configuration/master data only.

Historical validation results are recorded in PHASE-ONE.md. They do not certify transaction changes; use the transaction integration tests for current results.
