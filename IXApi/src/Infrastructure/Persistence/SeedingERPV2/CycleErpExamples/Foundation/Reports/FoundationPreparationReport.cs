namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports;
public sealed record FoundationPreparationReport(string Company, int FiscalYear, DateTime PreparedAtUtc, IReadOnlyList<string> CompletedSeeders, IReadOnlyList<string> MasterTables)
{
 public string Scope => "ConfigurationAndMasterData";
 public bool TransactionsExecuted => false;
}
