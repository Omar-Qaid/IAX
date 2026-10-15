using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IEcoResProductService : IBaseService<EcoResProduct>
{
    Task<List<EcoResProductDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<(EcoResProductDto? Product, string? Error)> CreateAsync(EcoResProductInputDto input, CancellationToken cancellationToken = default);
    Task<(EcoResProductDto? Product, string? Error)> UpdateAsync(long recId, EcoResProductInputDto input, CancellationToken cancellationToken = default);
    Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken cancellationToken = default);
}
