using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Reports;

public sealed record OrganizationPreparationReport(string Company, int FiscalYear, DateTime PreparedAtUtc,
    IReadOnlyList<string> CompletedSeeders, IReadOnlyList<string> MasterTables)
{
    public IReadOnlyList<string> PreparationSteps { get; init; } = [];
    /// <summary>The same seeders as <see cref="CompletedSeeders"/>, in the order the runner resolved them.</summary>
    public IReadOnlyList<SeederPreparationStep> SeederSteps { get; init; } = [];
    public OrganizationReconciliationReport? MasterData { get; init; }
    public string Scope => "ConfigurationAndMasterData";
    public bool TransactionsExecuted => false;
}
