namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;

public sealed record InventoryExampleOptions(int FiscalYear, string Actor);
public sealed record InventoryTransactionOptions(int FiscalYear, string Actor, IReadOnlyList<string>? ScenarioIds = null);
