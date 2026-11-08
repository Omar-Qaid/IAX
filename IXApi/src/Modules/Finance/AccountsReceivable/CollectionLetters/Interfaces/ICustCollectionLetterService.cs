namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public interface ICustCollectionLetterService
{
    Task<IReadOnlyList<string>> GenerateFirstLettersAsync(string account, string currency, DateOnly letterDate, CancellationToken ct);
    Task<bool> PostWarningAsync(string number, DateOnly postingDate, CancellationToken ct);
}
