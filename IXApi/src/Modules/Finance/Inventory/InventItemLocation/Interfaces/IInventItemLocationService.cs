using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventItemLocationService : IBaseService<InventItemLocation>
{
    Task<(ItemLocationRecordDto? Location, string? Error)> CreateAsync(InventItemLocationInputDto input, CancellationToken ct);
    Task<(ItemLocationRecordDto? Location, string? Error)> UpdateAsync(long recId, InventItemLocationInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}
