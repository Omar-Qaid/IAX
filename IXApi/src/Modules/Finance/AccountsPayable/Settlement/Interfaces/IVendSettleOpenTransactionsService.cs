using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The vendor transactions to settle against each other, marked by their <c>VendTrans</c> record
/// ids - D365's marking expressed as a request rather than stored rows.
/// </summary>
public sealed record VendSettleOpenInput(
    string VendAccount,
    DateTime? SettleDate,
    IReadOnlyList<long> TransRecIds);

public interface IVendSettleOpenTransactionsService : IBaseService<VendSettlement>
{
    /// <summary>Settles marked open vendor transactions against each other, with no payment involved.</summary>
    Task<PurchResult> SettleAsync(VendSettleOpenInput input, CancellationToken ct);

    /// <summary>The open transactions of one vendor, for marking an invoice to settle.</summary>
    Task<PurchResult> GetOpenTransactionsAsync(string accountNum, CancellationToken ct);
}
