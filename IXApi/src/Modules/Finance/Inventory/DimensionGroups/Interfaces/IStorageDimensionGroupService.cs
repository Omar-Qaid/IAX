using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IStorageDimensionGroupService : IBaseService<EcoResStorageDimensionGroup>
{
    Task<List<StorageDimensionGroupDto>> ListAsync(CancellationToken ct);
    Task<(StorageDimensionGroupDto? Group, string? Error)> CreateAsync(StorageDimensionGroupInputDto input, CancellationToken ct);
    Task<(StorageDimensionGroupDto? Group, string? Error)> UpdateAsync(long recId, StorageDimensionGroupInputDto input, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct);
}
