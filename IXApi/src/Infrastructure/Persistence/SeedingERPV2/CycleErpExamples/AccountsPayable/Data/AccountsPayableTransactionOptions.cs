namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
/// <summary>Explicit opt-in to business-service transactions; independent of master preparation.</summary>
public sealed record AccountsPayableTransactionOptions(int FiscalYear, string Actor, IReadOnlyList<string>? ScenarioIds = null);
