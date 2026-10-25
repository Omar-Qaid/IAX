using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventClosingQueryService : IBaseService<InventClosing>
{
    Task<IReadOnlyList<InventClosingRecordDto>> ListAsync(CancellationToken ct);
    Task<InventClosingRecordDto?> GetAsync(long recId, CancellationToken ct);
}
