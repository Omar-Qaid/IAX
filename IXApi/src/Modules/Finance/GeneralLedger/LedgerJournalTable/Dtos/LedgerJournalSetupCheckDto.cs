namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

public sealed record LedgerJournalSetupCheckDto(IReadOnlyList<string> Errors, IReadOnlyList<string> UnverifiedChecks)
{
    // Setup diagnostics alone never authorize posting.
    public bool PostingValidated => false;
}
