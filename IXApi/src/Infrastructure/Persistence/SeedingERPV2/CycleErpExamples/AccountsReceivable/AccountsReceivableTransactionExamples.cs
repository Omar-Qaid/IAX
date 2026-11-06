using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;

/// <summary>Explicit, resumable transaction examples. Each document is committed by its owning business service.</summary>
public sealed class AccountsReceivableTransactionExamples(ApplicationDbContext db, AccountsReceivableCycleSeeder preparation,
    AccountsReceivableTransactionFlow flow)
{
    public async Task<AccountsReceivableTransactionReport> RunAsync(AccountsReceivableTransactionOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (db.ChangeTracker.HasChanges()) throw new InvalidOperationException("Run AR examples in a clean DI scope.");
        var selected = options.ScenarioIds?.ToArray()
            ?? AccountsReceivableTransactionFlow.Scenarios.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (selected.Length == 0 || selected.Distinct(StringComparer.Ordinal).Count() != selected.Length
            || selected.Any(id => !AccountsReceivableTransactionFlow.Scenarios.ContainsKey(id)))
            throw new ArgumentException("Select distinct implemented AR scenario IDs.", nameof(options));
        db.ChangeTracker.Clear();
        await preparation.PrepareAsync(new(options.FiscalYear, options.Actor), ct);
        db.ChangeTracker.Clear();
        await using var seedLock = await new SqlServerSeederLock(db).AcquireLockAsync(120, ct);
        var reports = new List<AccountsReceivableScenarioReport>();
        foreach (var id in selected)
        {
            reports.Add(await flow.ExecuteAsync(options, id, ct));
            await new SeedExecutionHistory().RecordAsync(db, $"ERPV2.AR.{options.FiscalYear}.{id}", db.GetDataAreaId(), ct);
            db.ChangeTracker.Clear();
        }
        return new(db.GetDataAreaId(), options.FiscalYear, reports);
    }
}
