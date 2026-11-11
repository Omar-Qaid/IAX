namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;

public sealed record GeneralLedgerExampleOptions(int FiscalYear, string Actor);
public sealed record GeneralLedgerTransactionOptions(int FiscalYear, string Actor, IReadOnlyList<string>? ScenarioIds = null);
