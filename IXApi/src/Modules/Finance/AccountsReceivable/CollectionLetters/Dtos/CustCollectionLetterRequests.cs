namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed record CustCollectionLetterGenerateRequest(string AccountNum, string CurrencyCode, DateOnly LetterDate);
public sealed record CustCollectionLetterPostRequest(DateOnly PostingDate);
