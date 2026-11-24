namespace IAX.IXApi.Modules.Finance.AccountsPayable.CurrencyRevaluation;

/// <summary>The open vendor invoice restated at the rate date. Amounts are signed as stored: a payable is negative.</summary>
public sealed record VendRevaluationPreview(long TransactionRecId, string CurrencyCode, decimal OpenAmount,
    string AccountingCurrency, decimal PreviousAccountingValue, decimal RevaluedAccountingValue)
{
    public decimal AccountingDifference => RevaluedAccountingValue - PreviousAccountingValue;
}
public sealed record VendRevaluationPostRequest(DateOnly PostingDate, DateOnly RateDate);
public sealed record VendRevaluationReverseRequest(DateOnly PostingDate);
