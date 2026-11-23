using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;

public sealed class GeneralLedgerTransactionExamples(
    ApplicationDbContext db, GeneralLedgerCycleSeeder preparation, GeneralLedgerTransactionFlow flow,
    GeneralLedgerReferenceCycleFlow referenceCycle)
{
    public static IReadOnlyDictionary<string, string> Scenarios { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["01"] = "Expense reclassification and reversal",
        ["02"] = "Fifteen-step reference cycle to the 6,300 trial balance"
    };

    public async Task<(string Journal, string Reversal)> RunAsync(int fiscalYear, string actor, CancellationToken ct = default)
    {
        var report = await RunAsync(new GeneralLedgerTransactionOptions(fiscalYear, actor, ["01"]), ct);
        var scenario = report.Scenarios.Single();
        return (scenario.Journal, scenario.Reversal);
    }

    public async Task<GeneralLedgerExecutionReport> RunAsync(GeneralLedgerTransactionOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var selected = options.ScenarioIds?.ToArray() ?? Scenarios.Keys.Order(StringComparer.Ordinal).ToArray();
        if (selected.Length == 0 || selected.Distinct(StringComparer.Ordinal).Count() != selected.Length
            || selected.Any(id => !Scenarios.ContainsKey(id)))
            throw new ArgumentException("Select distinct implemented GL scenario IDs (01, 02).", nameof(options));
        if (db.ChangeTracker.HasChanges())
            throw new InvalidOperationException("Run GL examples in a clean DI scope.");
        db.ChangeTracker.Clear();
        await preparation.PrepareAsync(new(options.FiscalYear, options.Actor), ct);
        db.ChangeTracker.Clear();
        var reports = new List<GeneralLedgerScenarioReport>();
        foreach (var id in selected)
        {
            if (id == "01")
            {
                var result = await flow.RunAsync(options.FiscalYear, options.Actor, ct);
                reports.Add(new("01", result.Journal, result.Reversal, GeneralLedgerDemoData.ReclassificationAmount));
            }
            else reports.Add(await referenceCycle.RunAsync(options.FiscalYear, ct));
            db.ChangeTracker.Clear();
        }
        return new(db.GetDataAreaId(), options.FiscalYear, reports);
    }
}
