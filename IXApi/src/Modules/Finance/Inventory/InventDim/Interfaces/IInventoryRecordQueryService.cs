namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventoryRecordQueryService
{
    Task<IReadOnlyList<InventoryDimensionRecordDto>> DimensionsAsync(string? siteId, string? warehouseId, CancellationToken ct);
    Task<IReadOnlyList<ItemGroupAssignmentRecordDto>> ItemGroupsAsync(string? itemId, CancellationToken ct);
    Task<IReadOnlyList<ItemLocationRecordDto>> ItemLocationsAsync(string? itemId, CancellationToken ct);
    Task<IReadOnlyList<InventorySettlementRecordDto>> SettlementsAsync(string? itemId, string? voucher, CancellationToken ct);
}
