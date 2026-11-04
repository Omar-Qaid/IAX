using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
/// <summary>Explicit, resumable transaction examples. Each document is committed by its owning business service.</summary>
public sealed class AccountsPayableTransactionExamples(ApplicationDbContext db, AccountsPayableCycleSeeder preparation,
    AccountsPayableTransactionFlow flow)
{
    public async Task<AccountsPayableTransactionReport> RunAsync(AccountsPayableTransactionOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (db.ChangeTracker.HasChanges()) throw new InvalidOperationException("Run AP examples in a clean DI scope.");
        var selected = options.ScenarioIds?.ToArray() ?? AccountsPayableTransactionFlow.Scenarios.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (selected.Length == 0 || selected.Distinct(StringComparer.Ordinal).Count() != selected.Length
            || selected.Any(id => !AccountsPayableTransactionFlow.Scenarios.ContainsKey(id)))
            throw new ArgumentException("Select distinct implemented AP scenario IDs.", nameof(options));
        db.ChangeTracker.Clear();
        await preparation.PrepareAsync(new(options.FiscalYear, options.Actor), ct);
        db.ChangeTracker.Clear();
        await using var seedLock = await new SqlServerSeederLock(db).AcquireLockAsync(120, ct);
        var reports = new List<AccountsPayableScenarioReport>();
        foreach (var id in selected)
        {
            reports.Add(await flow.ExecuteAsync(options, id, ct));
            await new SeedExecutionHistory().RecordAsync(db, $"ERPV2.AP.{options.FiscalYear}.{id}", db.GetDataAreaId(), ct);
            db.ChangeTracker.Clear();
        }
        return new(db.GetDataAreaId(), options.FiscalYear, reports);
    }
}
