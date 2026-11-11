namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Arrival registration, the first step of D365's two-step receipt
/// (<c>Logic_Readonly.md</c> §10): the quantity is recorded as arrived before the product receipt
/// is posted.
/// </summary>
public interface IPurchRegistrationService
{
    /// <summary>
    /// Registers arrived quantities on a purchase order. This moves ordered inventory to
    /// <see cref="Common.StatusReceipt.Registered"/> and <b>posts nothing</b>: no ledger voucher, no
    /// vendor transaction, no physical value, and the line's remaining quantity to receive is
    /// unchanged. The product receipt afterwards consumes the registered quantity rather than
    /// adding to it, so a registration and its receipt are never two receipts of the same goods.
    /// </summary>
    Task<PurchResult> RegisterAsync(long purchTableRecId, PurchRegistrationPostDto input, CancellationToken ct);
}
