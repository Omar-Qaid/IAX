namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CurrencyRevaluation;

public sealed record CustRevaluationPreview(long TransactionRecId, string CurrencyCode, decimal OpenAmount,
    string AccountingCurrency, decimal PreviousAccountingValue, decimal RevaluedAccountingValue,
    string ReportingCurrency, decimal PreviousReportingValue, decimal RevaluedReportingValue)
{
    public decimal AccountingDifference => RevaluedAccountingValue - PreviousAccountingValue;
    public decimal ReportingDifference => RevaluedReportingValue - PreviousReportingValue;
}
