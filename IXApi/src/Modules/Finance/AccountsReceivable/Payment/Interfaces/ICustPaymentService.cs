namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

public interface ICustPaymentService
{
    Task<CustPaymentResult> PostAsync(CustPaymentPostDto input, CancellationToken ct);
}
