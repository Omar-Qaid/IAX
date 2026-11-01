using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// One item and inventory dimension in the inventory value report. Quantity and value are
/// reconstructed from inventory transactions, which are the authority; the on-hand summary is
/// reported beside them so any drift between the two is visible rather than hidden.
/// </summary>
public sealed record InventValueReportRowDto(
    string ItemId,
    string ItemName,
    string ItemGroupId,
    string InventDimId,
    string InventSiteId,
    string InventLocationId,
    string InventoryUnit,
    decimal PhysicalQuantity,
    decimal FinancialQuantity,
    decimal FinancialValue,
    decimal AverageUnitCost,
    decimal SummaryQuantity,
    decimal SummaryValue,
    decimal QuantityDifference,
    decimal ValueDifference);

/// <summary>
/// The ledger amount posted under one inventory posting type, so the report shows which postings
/// make up the inventory balance instead of asserting a single pass or fail.
/// </summary>
public sealed record InventValueLedgerBucketDto(LedgerPostingType PostingType, decimal AccountingCurrencyAmount);

public sealed record InventValueReportDto(
    DateOnly AsOfDate,
    int Page,
    int PageSize,
    int TotalRows,
    decimal TotalFinancialValue,
    decimal TotalSummaryValue,
    decimal LedgerInventoryAmount,
    decimal LedgerDifference,
    IReadOnlyList<InventValueLedgerBucketDto> LedgerBuckets,
    IReadOnlyList<InventValueReportRowDto> Rows);
