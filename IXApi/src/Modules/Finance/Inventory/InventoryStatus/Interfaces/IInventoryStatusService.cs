namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;

public interface IInventoryStatusService
{
    Task<List<InventoryStatusDto>?> ListAsync(CancellationToken ct);
    Task<(InventoryStatusDto? Row, string? Error)> SaveAsync(long? recId, InventoryStatusInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}
