using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

/// <summary>
/// One main account in the trial balance. Debit and Credit are the period's gross movements;
/// DebitBalance and CreditBalance present the net on its own side, as a trial balance reads.
/// </summary>
public sealed record LedgerTrialBalanceLineDto(
    string MainAccountId,
    string Name,
    decimal Debit,
    decimal Credit,
    decimal DebitBalance,
    decimal CreditBalance);

/// <summary>
/// Trial balance of posted accounting for one ledger, accounting-date range and posting layer.
/// Scope is stated on the result: a trial balance is only meaningful with its own filters.
/// </summary>
public sealed record LedgerTrialBalanceDto(
    DateTime FromDate,
    DateTime ToDate,
    CurrentToOperationsPostingLayer PostingLayer,
    string AccountingCurrency,
    IReadOnlyList<LedgerTrialBalanceLineDto> Lines,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal TotalDebitBalance,
    decimal TotalCreditBalance)
{
    /// <summary>Posted accounting nets to zero; a false value means the scope or the data needs investigation.</summary>
    public bool Balanced => TotalDebit - TotalCredit == 0m && TotalDebitBalance - TotalCreditBalance == 0m;
}
