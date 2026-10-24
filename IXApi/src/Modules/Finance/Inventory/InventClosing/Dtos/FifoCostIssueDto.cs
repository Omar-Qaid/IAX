namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record FifoCostIssueDto(long IssueRecId, string ItemId, string SiteId, DateTime FinancialDate,
    decimal Quantity, decimal RecognizedCost, decimal FifoCost, decimal RemainingAdjustment,
    IReadOnlyList<FifoCostAllocationDto> Allocations);
