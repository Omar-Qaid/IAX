using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchOrderService : IBaseService<PurchTable>
{
    Task<PurchResult> CreateAsync(PurchOrderCreateDto input, CancellationToken ct);
    Task<PurchResult> AddLineAsync(long recId, PurchLineInputDto input, CancellationToken ct);
    Task<PurchResult> GetAsync(long recId, CancellationToken ct);

    /// <summary>Cancels the quantity the order has not received, leaving the received part invoiceable.</summary>
    Task<PurchResult> CancelRemainderAsync(long recId, CancellationToken ct);
}
