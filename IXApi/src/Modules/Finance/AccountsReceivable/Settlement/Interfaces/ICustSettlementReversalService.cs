using IAX.IXApi.Infrastructure.Persistence.Services;

using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

public interface ICustSettlementReversalService : IBaseService<CustSettlement>
{
    /// <summary>
    /// Reverses the settlement posted under <paramref name="settlementVoucher"/>, restoring the
    /// customer transactions and their open balances.
    /// </summary>
    Task<CustPaymentResult> ReverseAsync(string settlementVoucher, DateTime? reversalDate, CancellationToken ct);
}
