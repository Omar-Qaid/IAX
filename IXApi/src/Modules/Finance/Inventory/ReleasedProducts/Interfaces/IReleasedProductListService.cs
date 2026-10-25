using IAX.IXApi.Infrastructure.Persistence.Services;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IReleasedProductListService : IBaseService<ReleasedItem>
{
    Task<List<ReleasedProductListDto>?> ListAsync(CancellationToken cancellationToken = default);
    Task<(ReleasedProductListDto? Product, string? Error)> CreateSharedAsync(CreateSharedProductDto input, CancellationToken cancellationToken = default);
    Task<(ReleasedProductListDto? Product, string? Error)> ReleaseAsync(ReleaseProductDto input, CancellationToken cancellationToken = default);
    Task<(ReleasedProductListDto? Product, string? Error)> ConfigureAsync(long recId, ConfigureReleasedProductDto input, CancellationToken cancellationToken = default);
    Task<(ReleasedProductListDto? Product, string? Error)> AssignGroupsAsync(long recId, ReleasedProductGroupAssignmentDto input, CancellationToken cancellationToken = default);
    Task<(ReleasedProductListDto? Product, string? Error)> AssignUnitsAsync(long recId, ReleasedProductUnitAssignmentDto input, CancellationToken cancellationToken = default);
    Task<ReleasedProductValidationDto?> ValidateAsync(long recId, CancellationToken cancellationToken = default);
    Task<ReleasedItemReadinessDto?> GetReadinessAsync(long recId, CancellationToken cancellationToken = default);
    Task<(ReleasedProductListDto? Product, string? Error)> CreateReleasedAsync(CreateReleasedProductDto input, CancellationToken cancellationToken = default);
    Task<ReleasedProductLookupDto?> GetLookupsAsync(CancellationToken cancellationToken = default);
}
