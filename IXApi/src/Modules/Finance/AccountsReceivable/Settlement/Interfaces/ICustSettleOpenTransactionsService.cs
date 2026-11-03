using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

/// <summary>
/// The transactions to settle against each other, marked by their <c>CustTrans</c> record ids. This
/// is D365's <c>SpecTrans</c> marking expressed as a request rather than stored rows.
/// </summary>
public sealed record CustSettleOpenInput(
    string CustAccount,
    DateTime? SettleDate,
    IReadOnlyList<long> TransRecIds);

public interface ICustSettleOpenTransactionsService : IBaseService<CustSettlement>
{
    /// <summary>Settles marked open transactions against each other, with no payment involved.</summary>
    Task<CustPaymentResult> SettleAsync(CustSettleOpenInput input, CancellationToken ct);
}
