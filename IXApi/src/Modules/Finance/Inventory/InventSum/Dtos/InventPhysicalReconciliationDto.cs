namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventPhysicalReconciliationRowDto(
    string ItemId, string InventDimId, string SiteId, string WarehouseId,
    decimal TransactionPhysical, decimal SummaryPhysical, decimal Difference,
    int UnclassifiedTransactionCount);

public sealed record InventPhysicalReconciliationDto(
    string CompanyId, int ComparedKeys, int DifferenceCount, int UnclassifiedTransactionCount,
    int MissingDimensionKeyCount,
    bool Complete, List<InventPhysicalReconciliationRowDto> Rows);
