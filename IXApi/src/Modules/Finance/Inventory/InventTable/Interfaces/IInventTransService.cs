namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventTransService
{
    Task<List<InventTransListDto>?> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<InventTransListDto>?> SearchAsync(string? itemId, string? sourceInventTransId,
        string? siteId, string? warehouseId, CancellationToken cancellationToken = default);
}
