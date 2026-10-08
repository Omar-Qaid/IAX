using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventLocationService : IBaseService<InventLocation>
{
    Task<List<InventLocationResponseDto>> ListAsync(CancellationToken ct);
    Task<object> LookupsAsync(CancellationToken ct);
    Task<LocationOperationResult> CreateAsync(LocationInputDto input, CancellationToken ct);
    Task<LocationOperationResult> UpdateAsync(long recId, LocationInputDto input, CancellationToken ct);
    Task<LocationOperationStatus> DeleteAsync(long recId, CancellationToken ct);
}
