using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;

/// <summary>Records successful preparation of configuration and master data only. No transaction is executed.</summary>
public sealed record InventoryPreparationReport(string Company, int FiscalYear, DateTime PreparedAtUtc,
    IReadOnlyList<InventoryPreparationStep> PreparationSteps,
    IReadOnlyList<InventorySeederStep> CompletedSeeders,
    InventoryReconciliationReport MasterData)
{
    public string Scope => ExampleIdentifiers.Scope;
    public bool TransactionsExecuted => false;
}

public sealed record InventoryPreparationStep(int Order, string Id, string Description);
public sealed record InventorySeederStep(int Order, string SeederId);

/// <summary>The outcome of one or more executed scenarios. Every figure is read back from the persisted rows.</summary>
public sealed record InventoryTransactionReport(string Company, int FiscalYear,
    IReadOnlyList<InventoryScenarioReport> Scenarios)
{
    public string Scope => "InventoryTransactions";
    public bool TransactionsExecuted => true;
}

public sealed record InventoryScenarioReport(string Id, string Description, string ItemId,
    IReadOnlyList<InventoryStageReport> Stages, decimal PhysicalQuantity, decimal InventoryValue,
    decimal CostOfGoodsSold, decimal CloseAdjustment, decimal ReservedQuantity,
    int PostedVouchers, int SettlementRows);

public sealed record InventoryStageReport(string CaseId, string Stage, string Reference, decimal? ExpectedQuantity,
    decimal? ActualQuantity, decimal? ExpectedValue, decimal? ActualValue, IReadOnlyList<string> Vouchers, string Outcome);
