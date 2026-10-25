using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IItemGroupAssignmentService : IBaseService<InventItemGroupItem>
{
    Task<(ItemGroupAssignmentRecordDto? Assignment, string? Error)> AssignAsync(string itemId, ItemGroupAssignmentInputDto input, CancellationToken ct);
}
