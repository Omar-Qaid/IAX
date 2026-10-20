namespace IAX.IXApi.Modules.Finance.Inventory;

public enum InventJournalNameDeleteResult { Deleted, NotFound, InUse }

public interface IInventJournalNameService
{
    Task<List<InventJournalNameDto>> ListAsync(CancellationToken ct);
    Task<InventJournalNameLookupsDto> LookupsAsync(CancellationToken ct);
    Task<(InventJournalNameDto? JournalName, string? Error)> CreateAsync(InventJournalNameInputDto input, CancellationToken ct);
    Task<(InventJournalNameDto? JournalName, string? Error)> UpdateAsync(long recId, InventJournalNameInputDto input, CancellationToken ct);
    Task<InventJournalNameDeleteResult> DeleteAsync(long recId, CancellationToken ct);
}
