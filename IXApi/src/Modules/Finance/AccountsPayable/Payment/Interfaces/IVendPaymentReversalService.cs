using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendPaymentReversalService : IBaseService<VendTrans>
{
    /// <summary>
    /// Reverses the posted vendor payment journal <paramref name="journalNum"/>: unwinds its
    /// settlements, posts the counter-voucher, and reverses the bank movement.
    /// </summary>
    Task<PurchResult> ReverseAsync(string journalNum, DateTime? reversalDate, CancellationToken ct);
}
