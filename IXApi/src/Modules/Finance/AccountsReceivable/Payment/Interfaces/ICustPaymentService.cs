using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

public interface ICustPaymentService : IBaseService<CustTrans>
{
    Task<CustPaymentResult> PostAsync(CustPaymentPostDto input, CancellationToken ct);
}
