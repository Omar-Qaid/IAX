namespace IAX.IXApi.Modules.Finance.AccountsPayable.CurrencyRevaluation;

public interface IVendCurrencyRevaluationService
{
    Task<VendRevaluationPreview?> PreviewAsync(long transactionRecId, DateOnly rateDate, CancellationToken ct);
    Task<string?> PostAsync(long transactionRecId, DateOnly postingDate, DateOnly rateDate, CancellationToken ct);
    Task<string?> ReverseAsync(long transactionRecId, DateOnly postingDate, CancellationToken ct);
}
