using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventSiteService : IBaseService<InventSite>
{
    Task<List<InventSiteDto>> ListAsync(CancellationToken ct);
    Task<(InventSiteDto? Site, bool Conflict)> CreateAsync(SiteInputDto input, CancellationToken ct);
    Task<(InventSiteDto? Site, bool CodeChanged)> UpdateAsync(long recId, SiteInputDto input, CancellationToken ct);
    Task<InventSiteDeleteResult> DeleteAsync(long recId, CancellationToken ct);
}
