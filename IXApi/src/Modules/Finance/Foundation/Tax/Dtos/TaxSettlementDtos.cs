namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

/// <summary>What to settle: the period interval that starts on <paramref name="FromDate"/>.</summary>
/// <param name="TaxPeriod">The settlement period (TaxPeriodHead.TaxPeriod).</param>
/// <param name="FromDate">The start date of the interval to settle, which identifies the interval.</param>
/// <param name="TransDate">The accounting date of the settlement voucher.</param>
public sealed record TaxSettlementInputDto(string TaxPeriod, DateOnly FromDate, DateOnly TransDate);

/// <summary>One tax account cleared by the settlement, with the balance that was moved off it.</summary>
public sealed record TaxSettlementAccountDto(
    string LedgerAccount,
    string Direction,
    decimal ClearedAmount);

/// <summary>
/// The outcome of one settlement run: the voucher posted, the interval settled, how many tax
/// transactions it covered, the balance moved to the settlement account, and the accounts cleared.
/// </summary>
public sealed record TaxSettlementResultDto(
    string TaxPeriod,
    DateOnly FromDate,
    DateOnly ToDate,
    int VersionNum,
    string Voucher,
    DateOnly TransDate,
    int TaxTransactionCount,
    decimal OutgoingTaxCleared,
    decimal IncomingTaxCleared,
    decimal SettlementAmount,
    bool OffsetTransactionsPosted,
    string SettlementAccount,
    IReadOnlyList<TaxSettlementAccountDto> ClearedAccounts);
