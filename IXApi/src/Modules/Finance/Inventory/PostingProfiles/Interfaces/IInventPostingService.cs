using IAX.IXApi.Infrastructure.Persistence.Services;
using InventPostingEntity = IAX.IXApi.Modules.Finance.Entities.InventPosting;

namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventPostingService : IBaseService<InventPostingEntity>
{
    Task<List<InventPostingRuleDto>?> ListAsync(int accountType, CancellationToken ct);
    Task<InventPostingLookupsDto?> LookupsAsync(CancellationToken ct);
    Task<(InventPostingRuleDto? Rule, string? Error)> CreateAsync(InventPostingRuleDto input, CancellationToken ct);
    Task<(InventPostingRuleDto? Rule, string? Error)> UpdateAsync(long recId, InventPostingRuleDto input, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct);
}
