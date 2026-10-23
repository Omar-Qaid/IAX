namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventTransService
{
    Task<List<InventTransListDto>?> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<InventTransListDto>?> SearchAsync(string? itemId, string? sourceInventTransId,
        string? siteId, string? warehouseId, CancellationToken cancellationToken = default);
    Task<InventTransPageDto?> SearchPageAsync(string? itemId, string? sourceInventTransId,
        string? siteId, string? warehouseId, DateTime? physicalFrom, DateTime? physicalTo,
        DateTime? financialFrom, DateTime? financialTo, int page, int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed record InventTransPageDto(int Page, int PageSize, int TotalCount, List<InventTransListDto> Rows);
