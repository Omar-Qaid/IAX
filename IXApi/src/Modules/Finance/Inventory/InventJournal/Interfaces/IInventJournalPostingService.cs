namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventJournalPostingService
{
    Task<(InventJournalPostResultDto? Result, string? Error)> PostAsync(string journalId, CancellationToken ct);
    Task<string?> FreezeCountAsync(string journalId, CancellationToken ct);
    Task<string?> ConfirmCountAsync(string journalId, IReadOnlyDictionary<long, decimal> quantities, CancellationToken ct);
    Task<string?> CancelCountAsync(string journalId, CancellationToken ct);
}
