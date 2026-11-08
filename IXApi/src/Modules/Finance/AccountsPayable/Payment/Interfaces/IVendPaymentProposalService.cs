namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// D365's vendor payment proposal: selects the open vendor invoices that match the given criteria
/// and states what paying them on the proposal date would cost. Read-only by design.
/// </summary>
public interface IVendPaymentProposalService
{
    Task<PurchResult> ProposeAsync(VendPaymentProposalRequestDto input, CancellationToken ct);
}
