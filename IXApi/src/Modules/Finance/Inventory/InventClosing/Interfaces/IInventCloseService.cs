namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventCloseService
{
    Task<(InventCloseResultDto? Result, string? Error)> CloseAsync(DateTime cutoff, CancellationToken ct = default);
    Task<(InventCloseResultDto? Result, string? Error)> CloseItemAsync(string itemId, DateTime cutoff, CancellationToken ct = default);
    Task<(InventCloseResultDto? Result, string? Error)> RecalculateAsync(DateTime cutoff, CancellationToken ct = default);
    Task<(InventCloseResultDto? Result, string? Error)> RecalculateItemAsync(string itemId, DateTime cutoff, CancellationToken ct = default);
    Task<(InventCloseCancelResultDto? Result, string? Error)> CancelAsync(long closingRecId, CancellationToken ct = default);
}
