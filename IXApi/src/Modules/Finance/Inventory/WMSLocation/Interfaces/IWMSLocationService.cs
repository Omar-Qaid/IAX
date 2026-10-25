using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IWMSLocationService : IBaseService<WMSLocation>
{
    Task<List<WMSLocationDto>> ListAsync(CancellationToken ct);
    Task<(WMSLocationDto? Location, string? Error)> CreateAsync(WMSLocationInputDto input, CancellationToken ct);
    Task<(WMSLocationDto? Location, string? Error)> UpdateAsync(long recId, WMSLocationInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}
