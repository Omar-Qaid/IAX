namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;

/// <summary>
/// Persisted configuration and master references captured during preparation. This is not reconciliation of posted
/// transactions: it states which rows the scenarios will resolve, so a missing prerequisite fails before any posting.
/// </summary>
public sealed record InventoryReconciliationReport(
    InventorySetupReference Configuration,
    IReadOnlyList<InventoryItemReference> Items,
    IReadOnlyList<InventoryPostingProfileReference> PostingProfiles);

public sealed record InventorySetupReference(long LedgerRecId, string AccountingCurrency, string SiteId,
    string WarehouseId, string InventDimId, string ItemGroup, string CostingMethod, string ItemTaxGroup,
    string InventoryAccount, string CostOfGoodsSoldAccount, long InventoryParametersRecId);

public sealed record InventoryItemReference(long RecId, string ItemId, long ProductRecId, string Name,
    string ProductType, string Unit, string ModelGroup, string ItemGroup, string ItemTaxGroup,
    bool Stocked, decimal PurchasePrice, decimal SalesPrice,
    long? StorageDimensionGroupRecId, long? TrackingDimensionGroupRecId);

public sealed record InventoryPostingProfileReference(long RecId, string AccountType, string ItemRelation,
    string MainAccount, long LedgerDimension);
