namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CurrencyRevaluation;

public interface ICustCurrencyRevaluationService
{
    Task<CustRevaluationPreview?> PreviewAsync(long transactionRecId, DateOnly rateDate, CancellationToken ct);
    Task<string?> PostAsync(long transactionRecId, DateOnly postingDate, DateOnly rateDate, CancellationToken ct);
    Task<string?> ReverseAsync(long transactionRecId, DateOnly postingDate, CancellationToken ct);
}
