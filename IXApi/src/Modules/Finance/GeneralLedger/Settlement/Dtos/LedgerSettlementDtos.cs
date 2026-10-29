namespace IAX.IXApi.Modules.Finance.GeneralLedger.Settlement;

/// <summary>Posted ledger entries to settle against each other, and the date to record.</summary>
public sealed record LedgerSettlementRequestDto(IReadOnlyList<long> AccountEntryRecIds, DateTime? SettleDate);

public sealed record LedgerSettlementResultDto(
    string SettleId,
    int EntriesSettled,
    decimal SettledAmount,
    DateTime SettleDate);

/// <summary>A posted entry on an account that no standing settlement covers.</summary>
public sealed record LedgerOpenItemDto(
    long AccountEntryRecId,
    string LedgerAccount,
    DateTime AccountingDate,
    string Voucher,
    string CurrencyCode,
    decimal AccountingCurrencyAmount,
    string Text);
