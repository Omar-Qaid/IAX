using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventOnHandService : IBaseService<InventSum>
{
    Task<List<InventOnHandDto>?> ListAsync(CancellationToken ct = default);
    Task<List<InventOnHandDto>?> SearchAsync(string? itemId, string? siteId, string? warehouseId,
        CancellationToken ct = default);
    Task<InventOnHandPageDto?> SearchPageAsync(string? itemId, string? itemGroupId, string? siteId,
        string? warehouseId, bool includeZero, int page, int pageSize, CancellationToken ct = default);
}

public sealed record InventOnHandPageDto(int Page, int PageSize, int TotalCount, List<InventOnHandDto> Rows);
