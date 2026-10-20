namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventOnHandDto
{
    public string Id { get; init; } = string.Empty;
    public long RecId { get; init; }
    public string ItemNumber { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string InventDimId { get; init; } = string.Empty;
    public string Site { get; init; } = string.Empty;
    public string Warehouse { get; init; } = string.Empty;
    public decimal PhysicalInventory { get; init; }
    public decimal PhysicalReserved { get; init; }
    public decimal AvailablePhysical { get; init; }
    public decimal OrderedReceipts { get; init; }
    public decimal OrderedIssues { get; init; }
    public decimal AvailableOrdered { get; init; }
    public decimal PostedValue { get; init; }
    public decimal PhysicalValue { get; init; }
}
