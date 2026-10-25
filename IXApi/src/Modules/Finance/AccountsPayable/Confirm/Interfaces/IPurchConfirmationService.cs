using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchConfirmationService : IBaseService<VendPurchOrderJour>
{
    Task<PurchResult> ConfirmAsync(long recId, CancellationToken ct);
}
