namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventCloseResultDto(
    long ClosingRecId,
    string Voucher,
    DateTime Cutoff,
    int RunNum,
    int IssuesSettled,
    int SettlementsCreated,
    int LedgerEntriesCreated,
    decimal TotalAdjustment);

public interface IInventCloseService
{
    Task<(InventCloseResultDto? Result, string? Error)> CloseAsync(DateTime cutoff, CancellationToken ct = default);
}
