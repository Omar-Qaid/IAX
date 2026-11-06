# Accounts Receivable examples

This package has two explicit entry points. `AccountsReceivableCycleSeeder.PrepareAsync` prepares configuration and master data only. `AccountsReceivableTransactionExamples.RunAsync` prepares those prerequisites and executes the selected transaction examples through the owning business services. Neither entry point runs at application startup.

See [phase two](PHASE-TWO.md) for the opt-in transaction API, implemented scenarios and boundaries.

## How this relates to the purchase example

The sales example is deliberately built on top of the purchase example rather than beside it. The demo items have to be bought and received before a sale has anything to deliver, so `AddAccountsReceivablePhaseOneSeeding` registers the AP phase first and `AddAccountsReceivableTransactionExamples` registers the AP transaction examples too.

What is reused rather than seeded again: the legal entity, ledger, chart of accounts, fiscal calendar, the account structure covering the purchase accounts, the company bank accounts, the Riyadh geography, the site and warehouse, the inventory dimension, the released items with their item and model groups, the NET30 payment term, and the shared item tax group.

What the sales example owns: its own main accounts in disjoint code ranges, a second `ARV2` account structure for them, the customer and sales document number sequences, the AR journal names, the output tax code and its posting group, the customer group, the customer posting profile, the AR and sales parameters, the collection method, the sales cash discount, the sales inventory posting rules, the return handling codes, the customers with their parties, premises and contacts, and the sales trade agreements.

## Implemented preparation

1. Validate the embedded example manifests and the existing company context.
2. Run the GL, Tax, Inventory, Cash/Bank, AP and AR seeders in their actual dependency order.
3. Validate the stored configuration and master relationships inside the seed transaction.
4. Commit the masters and history atomically and return their references and completed seeder order.

The module seeders own all writes; this folder does not duplicate their setup or insert business transactions. The existing conflict checks, company scope, database seeder lock and safe rerun behavior apply.

## Invocation

```csharp
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;

services.AddAccountsReceivableConfigurationAndMasterDataExamples();

// Use a fresh scope with the intended ApplicationDbContext company execution context.
using var scope = serviceProvider.CreateScope();
var examples = scope.ServiceProvider.GetRequiredService<AccountsReceivableCycleSeeder>();
var report = await examples.PrepareAsync(new AccountsReceivableExampleOptions(2026, actorUserId), ct);
```

Registration is opt-in and idempotent. No application startup hook executes it. The report scope is `ConfigurationAndMasterData` and `TransactionsExecuted` is always `false`. `PreparedAtUtc` is the report timestamp, not a document posting date.

## Data and ownership

- `Data/customers.json`: the two named fictional customers and their setup keys.
- `Data/sales-prices.json`: the three sales quotes (SAR 40, SAR 500, USD 11).
- `Data/scenarios.json`: six configuration/master preparation steps, with transaction scenarios disabled.

These embedded files describe and verify the module-owned fixtures. They are not a second data insertion mechanism. Edits that diverge from the owning seeders are rejected before writes; stored values are checked again before commit. Database RecIds are resolved from company and stable business keys, never copied from JSON.

`Reports/ScenarioExecutionReport.cs` records preparation only. `AccountsReceivableReconciliationReport` contains master-data references, not posted reconciliation balances. A report is returned only after the seed transaction commits successfully.

Customer identities and premises are fictional demonstration data.

## Why the sales quotations sit above cost

Each SAR quotation is validated to be above the purchase price of the same item, so the cycle produces a positive margin and the cost-of-goods-sold and revenue accounts can be told apart in the reconciliation. A quotation at or below cost would still post, but it would hide a swapped revenue and cost account behind a zero.

See [the realistic demo](../../REAL-EXAMPLE.md) and [phase-one setup](../../PHASE-ONE.md).
