namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Reports;

public sealed record GeneralLedgerPreparationReport(string Company, int FiscalYear, IReadOnlyList<string> CompletedSeeders);
public sealed record GeneralLedgerScenarioReport(string Id, string Journal, string Reversal, decimal Amount);
public sealed record GeneralLedgerExecutionReport(string Company, int FiscalYear, IReadOnlyList<GeneralLedgerScenarioReport> Scenarios);
