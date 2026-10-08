namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

public interface ISalesOrderConfirmationService
{
    Task<ConfirmationResult> ListAsync(long recId, CancellationToken ct);
    Task<ConfirmationResult> GetAsync(long recId, long confirmationRecId, CancellationToken ct);
    Task<ConfirmationResult> PostAsync(long recId, PostConfirmationRequest? request, CancellationToken ct);
}
