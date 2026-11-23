using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Reports;

public sealed record WorkflowPreparationReport(string Company, int FiscalYear, DateTime PreparedAtUtc,
    IReadOnlyList<string> CompletedSeeders, IReadOnlyList<string> MasterTables)
{
    public IReadOnlyList<string> PreparationSteps { get; init; } = [];
    /// <summary>The same seeders as <see cref="CompletedSeeders"/>, in the order the runner resolved them.</summary>
    public IReadOnlyList<SeederPreparationStep> SeederSteps { get; init; } = [];
    public WorkflowReconciliationReport? MasterData { get; init; }
    public string Scope => "ConfigurationAndMasterData";
    public bool TransactionsExecuted => false;
}
