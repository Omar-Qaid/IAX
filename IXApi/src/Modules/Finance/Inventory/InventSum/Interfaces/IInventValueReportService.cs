namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventValueReportService
{
    Task<InventValueReportDto?> RunAsync(DateOnly asOfDate, string? itemId, string? itemGroupId,
        string? siteId, string? warehouseId, bool includeZero, int page, int pageSize, CancellationToken ct = default);
}
