namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

/// <summary>
/// What the static checks can see about a journal, without attempting to post it: the setup check
/// and the voucher arithmetic, combined.
///
/// <see cref="Valid"/> means no static check objected, not that posting will succeed — resolving
/// accounts, applying the account structures and building balanced vouchers all happen in the
/// posting path. Simulate is the authoritative answer, because it runs that path.
/// </summary>
public sealed record LedgerJournalValidateResultDto(
    string JournalNum,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> UnverifiedChecks)
{
    public bool Valid => Errors.Count == 0;

    public string Scope =>
        "Static setup and voucher arithmetic only. Simulate posting for an authoritative answer; "
        + "neither creates posted accounting.";
}

/// <summary>One account entry a simulated posting would create.</summary>
public sealed record LedgerJournalSimulatedEntryDto(
    string Voucher,
    DateTime AccountingDate,
    string LedgerAccount,
    string PostingType,
    string CurrencyCode,
    decimal AccountingCurrencyAmount,
    string Text);

/// <summary>
/// The outcome of running the real posting path and rolling it back. <see cref="Posted"/> is always
/// false: nothing reaches the database, which is the point.
/// </summary>
public sealed record LedgerJournalSimulateResultDto(
    string JournalNum,
    bool WouldPost,
    string? Refusal,
    int Vouchers,
    int AccountEntries,
    decimal TotalDebit,
    decimal TotalCredit,
    IReadOnlyList<LedgerJournalSimulatedEntryDto> Entries)
{
    public bool Posted => false;

    public string Scope =>
        "The posting path ran inside a transaction that was rolled back. No posted accounting, "
        + "journal status change or voucher number was kept.";
}
