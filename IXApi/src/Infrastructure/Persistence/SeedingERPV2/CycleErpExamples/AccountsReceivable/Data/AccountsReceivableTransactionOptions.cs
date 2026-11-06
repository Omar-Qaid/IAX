namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
/// <summary>Explicit opt-in to business-service transactions; independent of master preparation.</summary>
public sealed record AccountsReceivableTransactionOptions(int FiscalYear, string Actor, IReadOnlyList<string>? ScenarioIds = null);
