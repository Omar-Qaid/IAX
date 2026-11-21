namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

/// <summary>
/// One posted general ledger line as the voucher transactions inquiry reads it: the journal and
/// voucher it belongs to, the account it hit, and its amounts in transaction and accounting currency.
/// </summary>
public sealed record LedgerTransVoucherRow(
    long RecId,
    string JournalNumber,
    string Voucher,
    DateTime TransDate,
    bool YearClosed,
    string LedgerAccount,
    string AccountName,
    string Description,
    string CurrencyCode,
    decimal AmountCur,
    decimal AmountMst,
    string PostingType);
