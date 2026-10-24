namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventJournalPostingService
{
    Task<(InventJournalPostResultDto? Result, string? Error)> PostAsync(string journalId, CancellationToken ct);
}
