namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventoryMarkingDto(
    string ItemId,
    string IssueInventTransId,
    string ReceiptInventTransId,
    decimal Quantity,
    bool IsMarked);

public sealed record InventoryMarkingRowDto(
    string ItemId,
    string IssueInventTransId,
    string ReceiptInventTransId,
    decimal Quantity,
    bool IsOpen);
