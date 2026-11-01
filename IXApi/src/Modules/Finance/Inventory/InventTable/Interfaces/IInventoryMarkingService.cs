namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventoryMarkingService
{
    Task<(InventoryMarkingDto? Result, string? Error)> MarkAsync(string issueInventTransId,
        string receiptInventTransId, CancellationToken ct = default);
    Task<(InventoryMarkingDto? Result, string? Error)> UnmarkAsync(string inventTransId, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryMarkingRowDto>?> ListAsync(string? itemId, CancellationToken ct = default);
}
