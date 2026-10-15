using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface ITrackingDimensionGroupService : IBaseService<EcoResTrackingDimensionGroup>
{
    Task<List<TrackingDimensionGroupDto>> ListAsync(CancellationToken ct);
    Task<(TrackingDimensionGroupDto? Group, string? Error)> CreateAsync(TrackingDimensionGroupInputDto input, CancellationToken ct);
    Task<(TrackingDimensionGroupDto? Group, string? Error)> UpdateAsync(long recId, TrackingDimensionGroupInputDto input, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct);
}
