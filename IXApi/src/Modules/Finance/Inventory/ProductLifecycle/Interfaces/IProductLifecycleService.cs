using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

public interface IProductLifecycleService : IBaseService<EcoResProductLifecycleState>
{
    Task<List<ProductLifecycleStateDto>> ListAsync(CancellationToken ct);
    Task<(ProductLifecycleStateDto? Row, string? Error)> SaveAsync(long? recId, ProductLifecycleStateInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
    Task<(ReleasedProductLifecycleDto? Item, string? Error)> AssignAsync(long releasedProductRecId, string stateId, CancellationToken ct);
}
