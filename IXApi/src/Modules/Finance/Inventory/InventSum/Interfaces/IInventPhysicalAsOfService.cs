namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventPhysicalAsOfService
{
    Task<InventPhysicalAsOfPageDto?> SearchAsync(DateOnly asOfDate, string? itemId,
        string? siteId, string? warehouseId, int page, int pageSize, CancellationToken ct = default);
}
