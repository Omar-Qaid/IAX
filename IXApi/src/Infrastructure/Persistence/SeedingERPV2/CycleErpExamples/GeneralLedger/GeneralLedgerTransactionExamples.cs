using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;

public sealed class GeneralLedgerTransactionExamples(
    ApplicationDbContext db, GeneralLedgerCycleSeeder preparation, GeneralLedgerTransactionFlow flow)
{
    public async Task<(string Journal, string Reversal)> RunAsync(int fiscalYear, string actor, CancellationToken ct = default)
    {
        var report = await RunAsync(new GeneralLedgerTransactionOptions(fiscalYear, actor), ct);
        var scenario = report.Scenarios.Single();
        return (scenario.Journal, scenario.Reversal);
    }

    public async Task<GeneralLedgerExecutionReport> RunAsync(GeneralLedgerTransactionOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var selected = options.ScenarioIds?.ToArray() ?? ["01"];
        if (selected.Length != 1 || selected[0] != "01")
            throw new ArgumentException("Select implemented GL scenario 01.", nameof(options));
        if (db.ChangeTracker.HasChanges())
            throw new InvalidOperationException("Run GL examples in a clean DI scope.");
        db.ChangeTracker.Clear();
        await preparation.PrepareAsync(new(options.FiscalYear, options.Actor), ct);
        db.ChangeTracker.Clear();
        var result = await flow.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(db.GetDataAreaId(), options.FiscalYear,
            [new("01", result.Journal, result.Reversal, GeneralLedgerDemoData.ReclassificationAmount)]);
    }
}
