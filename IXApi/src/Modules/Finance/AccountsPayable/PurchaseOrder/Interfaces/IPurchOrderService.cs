using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchOrderService : IBaseService<PurchTable>
{
    Task<PurchResult> CreateAsync(PurchOrderCreateDto input, CancellationToken ct);
    Task<PurchResult> AddLineAsync(long recId, PurchLineInputDto input, CancellationToken ct);
    Task<PurchResult> GetAsync(long recId, CancellationToken ct);
}
