namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventJournalService
{
    Task<List<InventJournalTableDto>> ListAsync(CancellationToken ct);
    Task<InventJournalTableDto?> GetAsync(string journalId, CancellationToken ct);
    Task<InventJournalLookupsDto> LookupsAsync(CancellationToken ct);
    Task<(InventJournalTableDto? Journal, string? Error)> CreateAsync(InventJournalCreateDto input, CancellationToken ct);
    Task<(InventJournalTableDto? Journal, string? Error)> AddLineAsync(string journalId, InventJournalLineInputDto input, CancellationToken ct);
    Task<(InventJournalTableDto? Journal, string? Error)> UpdateLineAsync(string journalId, long lineRecId, InventJournalLineInputDto input, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteLineAsync(string journalId, long lineRecId, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteAsync(string journalId, CancellationToken ct);
}

public interface IInventJournalPostingService
{
    Task<(InventJournalPostResultDto? Result, string? Error)> PostAsync(string journalId, CancellationToken ct);
}
