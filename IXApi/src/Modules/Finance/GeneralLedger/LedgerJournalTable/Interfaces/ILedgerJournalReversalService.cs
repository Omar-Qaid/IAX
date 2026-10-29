namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

public interface ILedgerJournalReversalService
{
    /// <summary>
    /// Reverses a posted general journal by creating and posting a mirrored journal.
    /// Returns (null, null) when the journal does not exist in the selected company.
    /// </summary>
    Task<(LedgerJournalPostResultDto? Result, string? Error)> ReverseAsync(long recId, DateTime? reversalDate, CancellationToken ct);
}
