namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

/// <summary>Posted result of a general journal: one voucher can produce several account entries.</summary>
public sealed record LedgerJournalPostResultDto(
    string JournalNum,
    int VouchersPosted,
    int AccountEntriesPosted,
    decimal TotalDebit,
    decimal TotalCredit,
    DateTime PostedDateTime);
