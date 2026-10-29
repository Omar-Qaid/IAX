using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

public interface ILedgerTrialBalanceService
{
    /// <summary>
    /// Trial balance of posted accounting for the selected company's ledger, between two
    /// accounting dates, in one posting layer.
    /// </summary>
    Task<(LedgerTrialBalanceDto? Result, string? Error)> GetAsync(DateTime fromDate, DateTime toDate,
        CurrentToOperationsPostingLayer postingLayer, CancellationToken ct);
}
