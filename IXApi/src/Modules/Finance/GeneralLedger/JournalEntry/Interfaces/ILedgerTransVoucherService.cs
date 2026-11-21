namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

public interface ILedgerTransVoucherService
{
    /// <summary>
    /// Posted ledger lines of the selected company, optionally narrowed to one voucher and an
    /// accounting date range. Posted accounting is read only; nothing here changes a voucher.
    /// </summary>
    Task<(IReadOnlyList<LedgerTransVoucherRow>? Rows, string? Error)> GetAsync(
        string? voucher, DateTime? fromDate, DateTime? toDate, CancellationToken ct);
}
