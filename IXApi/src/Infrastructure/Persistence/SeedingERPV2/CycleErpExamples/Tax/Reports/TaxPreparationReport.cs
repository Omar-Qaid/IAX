using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Reports;

public sealed record TaxPreparationReport(string Company, int FiscalYear, DateTime PreparedAtUtc,
    IReadOnlyList<string> CompletedSeeders, IReadOnlyList<string> MasterTables)
{
    public IReadOnlyList<string> PreparationSteps { get; init; } = [];
    /// <summary>The same seeders as <see cref="CompletedSeeders"/>, in the order the runner resolved them.</summary>
    public IReadOnlyList<SeederPreparationStep> SeederSteps { get; init; } = [];
    public TaxReconciliationReport? MasterData { get; init; }
    public string Scope => "ConfigurationAndMasterData";
    public bool TransactionsExecuted => false;
}
