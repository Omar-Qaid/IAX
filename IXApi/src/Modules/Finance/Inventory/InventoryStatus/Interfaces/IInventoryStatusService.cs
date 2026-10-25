using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;

public interface IInventoryStatusService : IBaseService<WHSInventStatus>
{
    Task<List<InventoryStatusDto>?> ListAsync(CancellationToken ct);
    Task<(InventoryStatusDto? Row, string? Error)> SaveAsync(long? recId, InventoryStatusInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}
