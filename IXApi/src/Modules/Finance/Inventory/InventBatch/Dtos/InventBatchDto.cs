namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventBatchDto
{
    public long RecId { get; init; }
    public string InventBatchId { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public DateTime? ProdDate { get; init; }
    public DateTime? ExpDate { get; init; }
}

public sealed class InventBatchInputDto
{
    public string InventBatchId { get; init; } = string.Empty;
    public string ItemId { get; init; } = string.Empty;
    public DateTime? ProdDate { get; init; }
    public DateTime? ExpDate { get; init; }
}

