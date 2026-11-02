using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventCloseResultDto(
    long ClosingRecId,
    string Voucher,
    DateTime Cutoff,
    int RunNum,
    InventAdjustmentType RunType,
    int IssuesSettled,
    int SettlementsCreated,
    int LedgerEntriesCreated,
    decimal TotalAdjustment)
{
    public IReadOnlyList<InventCloseUnsettledIssueDto> UnsettledIssues { get; init; } = [];
}

public sealed record InventCloseUnsettledIssueDto(long TransactionRecId, string ItemId, decimal RemainingQuantity);

public sealed record InventCloseCancelResultDto(
    long CancellationRecId,
    string Voucher,
    long CancelledClosingRecId,
    int CancelledRunNum,
    int RunNum,
    int SettlementsReversed,
    int LedgerEntriesCreated,
    decimal TotalReversed);
