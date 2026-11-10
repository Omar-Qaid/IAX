using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

/// <summary>Explicit, resumable transaction examples. Each document is committed by its owning business service.</summary>
public sealed class InventoryTransactionExamples(ApplicationDbContext db, InventoryCycleSeeder preparation,
    InventoryTransactionFlow flow)
{
    public async Task<InventoryTransactionReport> RunAsync(InventoryTransactionOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (db.ChangeTracker.HasChanges()) throw new InvalidOperationException("Run Inventory examples in a clean DI scope.");
        var selected = options.ScenarioIds?.ToArray()
            ?? InventoryTransactionFlow.Scenarios.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (selected.Length == 0 || selected.Distinct(StringComparer.Ordinal).Count() != selected.Length
            || selected.Any(id => !InventoryTransactionFlow.Scenarios.ContainsKey(id)))
            throw new ArgumentException("Select distinct implemented Inventory scenario IDs.", nameof(options));

        db.ChangeTracker.Clear();
        await preparation.PrepareAsync(new(options.FiscalYear, options.Actor), ct);
        db.ChangeTracker.Clear();
        await using var seedLock = await new SqlServerSeederLock(db).AcquireLockAsync(120, ct);
        var reports = new List<InventoryScenarioReport>();
        foreach (var id in selected)
        {
            reports.Add(await flow.ExecuteAsync(options, id, ct));
            await new SeedExecutionHistory().RecordAsync(db, $"ERPV2.Inventory.{options.FiscalYear}.{id}", db.GetDataAreaId(), ct);
            db.ChangeTracker.Clear();
        }
        return new(db.GetDataAreaId(), options.FiscalYear, reports);
    }
}
