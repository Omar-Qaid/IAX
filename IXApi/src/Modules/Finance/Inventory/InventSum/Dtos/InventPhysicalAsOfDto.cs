namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventPhysicalAsOfRowDto(
    string ItemId, string InventDimId, string SiteId, string WarehouseId,
    decimal PhysicalQuantity, int UnverifiedStateCount, bool DimensionFound);

public sealed record InventPhysicalAsOfPageDto(
    DateOnly AsOfDate, int Page, int PageSize, int TotalCount,
    List<InventPhysicalAsOfRowDto> Rows);
