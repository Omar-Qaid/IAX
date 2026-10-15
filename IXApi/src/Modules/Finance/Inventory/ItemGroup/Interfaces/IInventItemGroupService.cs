using IAX.IXApi.Infrastructure.Persistence.Services;
using InventItemGroupEntity = IAX.IXApi.Modules.Finance.Entities.InventItemGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventItemGroupService : IBaseService<InventItemGroupEntity>
{
    Task<List<InventItemGroupRecordDto>?> ListAsync(CancellationToken ct);
    Task<List<InventItemGroupPostingDto>?> LedgerAccountsAsync(CancellationToken ct);
    Task<(InventItemGroupRecordDto? Group, string? Error)> CreateAsync(InventItemGroupRecordDto input, CancellationToken ct);
    Task<(InventItemGroupRecordDto? Group, string? Error)> UpdateAsync(long recId, InventItemGroupRecordDto input, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct);
}
