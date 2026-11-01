using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendSettlementReversalService : IBaseService<VendSettlement>
{
    /// <summary>
    /// Reverses the settlement recorded under <paramref name="voucher"/>, restoring the vendor
    /// transactions and their open balances.
    /// </summary>
    Task<PurchResult> ReverseAsync(string voucher, DateTime? reversalDate, CancellationToken ct);
}
