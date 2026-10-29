using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventClosingRecordDto
{
    public long RecId { get; init; }
    public DateTime TransDate { get; init; }
    public DateTime Executed { get; init; }
    public string Voucher { get; init; } = string.Empty;
    public string ScopeItemId { get; init; } = string.Empty;
    public string PeriodCode { get; init; } = string.Empty;
    public int RunNum { get; init; }
    public InventAdjustmentType AdjustmentType { get; init; }
    public InventCostStatus InventCostStatus { get; init; }
    public bool Cancellation { get; init; }
    public bool CancelClosing { get; init; }
    public bool RunRecalculation { get; init; }
    public string? Notes { get; init; }
}
