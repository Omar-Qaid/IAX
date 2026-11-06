using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;

/// <summary>Records successful preparation of masters only. No transaction scenario is executed.</summary>
public sealed record ScenarioExecutionReport(string Company, int FiscalYear, DateTime PreparedAtUtc,
    IReadOnlyList<ConfigurationPreparationStep> PreparationSteps,
    IReadOnlyList<SeederPreparationStep> CompletedSeeders,
    AccountsReceivableReconciliationReport MasterData)
{
    public string Scope => ExampleIdentifiers.Scope;
    public bool TransactionsExecuted => false;
}

public sealed record ConfigurationPreparationStep(int Order, string Id, string Description);
public sealed record SeederPreparationStep(int Order, string SeederId);
