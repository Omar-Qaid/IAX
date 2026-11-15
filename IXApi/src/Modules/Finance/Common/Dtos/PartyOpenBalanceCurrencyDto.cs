namespace IAX.IXApi.Modules.Finance.Common;

public sealed record OpenBalanceCurrencyRowDto(
    string CurrencyCode, decimal AmountCur, decimal AmountMst, decimal? AmountReporting);

/// <summary>Read model for the standard Cust/VendOpenBalanceCurrency inquiries, not a stored entity.</summary>
public sealed record PartyOpenBalanceCurrencyDto(
    long RecId, string AccountNum, string Name, string NameAlias, string CurrencyCode,
    string AccountingCurrency, string ReportingCurrency,
    decimal BalanceMst, decimal? BalanceReporting, decimal CreditMax,
    bool Reconciled, IReadOnlyList<string> RelatedAccounts,
    IReadOnlyList<OpenBalanceCurrencyRowDto> Currencies);
