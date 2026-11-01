using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

public interface ICustPaymentReversalService : IBaseService<CustTrans>
{
    /// <summary>
    /// Reverses the posted customer payment journal <paramref name="journalNum"/>: unwinds its
    /// settlements, posts the counter-voucher, and reverses the bank movement.
    /// </summary>
    Task<CustPaymentResult> ReverseAsync(string journalNum, DateTime? reversalDate, CancellationToken ct);
}
