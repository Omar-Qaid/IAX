using IAX.IXApi.Infrastructure.Persistence.Services;
using InventParametersEntity = IAX.IXApi.Modules.Finance.Entities.InventParameters;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventParametersService : IBaseService<InventParametersEntity>
{
    Task<InventParametersDto?> GetAsync(CancellationToken ct = default);
    Task<InventParametersLookupsDto?> GetLookupsAsync(CancellationToken ct = default);
    Task<(InventParametersDto? Value, string? Error)> SaveAsync(InventParametersDto input, CancellationToken ct = default);
    Task<InventoryReadinessDto?> GetReadinessAsync(CancellationToken ct = default);
}
