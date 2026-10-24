namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchConfirmationService
{
    Task<PurchResult> ConfirmAsync(long recId, CancellationToken ct);
}
