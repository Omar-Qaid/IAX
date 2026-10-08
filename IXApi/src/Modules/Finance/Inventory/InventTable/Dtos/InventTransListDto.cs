namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventTransListDto
{
    public long RecId { get; init; }
    public string ItemNumber { get; init; } = string.Empty;
    public DateTime? PhysicalDate { get; init; }
    public DateTime? FinancialDate { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
    public string InventTransId { get; init; } = string.Empty;
    public string ReceiptStatus { get; init; } = string.Empty;
    public string IssueStatus { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal UnitCost { get; init; }
    public decimal CostAmount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string Site { get; init; } = string.Empty;
    public string Warehouse { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public DateTime? ExpectedDate { get; init; }
    public string Voucher { get; init; } = string.Empty;
    public string DataAreaId { get; init; } = string.Empty;
}
