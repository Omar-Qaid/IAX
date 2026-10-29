using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendPaymentService : IBaseService<VendTrans>
{
    Task<PurchResult> PostAsync(VendPaymentPostDto input, CancellationToken ct);
}
