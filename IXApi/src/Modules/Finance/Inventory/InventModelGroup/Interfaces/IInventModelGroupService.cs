using IAX.IXApi.Infrastructure.Persistence.Services;
using InventModelGroupEntity = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventModelGroupService : IBaseService<InventModelGroupEntity>
{
    Task<List<InventModelGroupRecordDto>?> ListAsync(CancellationToken ct);
    Task<(InventModelGroupRecordDto? Group, string? Error)> CreateAsync(InventModelGroupRecordDto input, CancellationToken ct);
    Task<(InventModelGroupRecordDto? Group, string? Error)> UpdateAsync(long recId, InventModelGroupRecordDto input, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct);
}
