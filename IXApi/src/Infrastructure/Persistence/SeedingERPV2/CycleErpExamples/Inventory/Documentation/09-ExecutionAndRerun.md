# 09 — Execution and rerun

Neither registration seeds anything at application startup. Both entry points are explicit.

## Preparation only

```csharp
services.AddInventoryConfigurationAndMasterDataExamples();

var report = await scope.ServiceProvider.GetRequiredService<InventoryCycleSeeder>()
    .PrepareAsync(new InventoryExampleOptions(2026, actorUserId), ct);
```

`PrepareAsync` validates the embedded manifests before any write, runs the dependency-ordered seeders inside the
seed transaction, and returns the ordered preparation steps together with the master references it captured.
It creates no stock, no invoice and no accounting entry.

## Transactions

```csharp
services.AddInventoryTransactionExamples();

var report = await scope.ServiceProvider.GetRequiredService<InventoryTransactionExamples>()
    .RunAsync(new InventoryTransactionOptions(2026, actorUserId, ["01"]), ct);
```

Omitting the scenario ids selects every implemented scenario. An unimplemented or duplicated id is rejected before
anything runs. The host supplies `ApplicationDbContext`, the company context, the current user and the Finance
mappings, exactly as for the AP and AR packages.

## Rerun behavior

Preparation inserts missing rows and refuses a conflicting existing value rather than overwriting it; consumed number
sequences are preserved. Transactions resume from stable document keys, and a completed close is reconciled rather
than repeated.
