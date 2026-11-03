namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventoryMarkingService
{
    Task<(InventoryMarkingDto? Result, string? Error)> MarkAsync(string issueInventTransId,
        string receiptInventTransId, CancellationToken ct = default);

    /// <summary>
    /// Marks a chosen movement on either side. A side already split across several movements has
    /// no single movement to mark, so the caller names it by <c>InventTrans.RecId</c>; null keeps
    /// the unsplit behavior.
    /// </summary>
    Task<(InventoryMarkingDto? Result, string? Error)> MarkAsync(string issueInventTransId,
        string receiptInventTransId, long? issueTransRecId, long? receiptTransRecId,
        CancellationToken ct = default);
    Task<(InventoryMarkingDto? Result, string? Error)> UnmarkAsync(string inventTransId, CancellationToken ct = default);
    Task<(InventoryMarkingDto? Result, string? Error)> UnmarkAsync(string inventTransId, long? inventTransRecId,
        CancellationToken ct);
    Task<IReadOnlyList<InventoryMarkingRowDto>?> ListAsync(string? itemId, CancellationToken ct = default);
}
