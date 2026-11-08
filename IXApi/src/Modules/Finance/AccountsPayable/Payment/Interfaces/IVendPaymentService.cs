using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendPaymentService : IBaseService<VendTrans>
{
    Task<PurchResult> PostAsync(VendPaymentPostDto input, CancellationToken ct);

    /// <summary>
    /// Receives money a vendor owes us back and settles it against the selected open vendor debit,
    /// which is the payable mirror of the customer refund.
    /// </summary>
    Task<PurchResult> RefundAsync(VendRefundPostDto input, CancellationToken ct);
}
