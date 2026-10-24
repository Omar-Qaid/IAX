namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventCountGroupService
{
    Task<IReadOnlyList<InventCountGroupRecordDto>> ListAsync(CancellationToken ct);
    Task<InventCountGroupRecordDto?> GetAsync(long recId, CancellationToken ct);
    Task<(InventCountGroupRecordDto? Group, string? Error)> CreateAsync(InventCountGroupInputDto input, CancellationToken ct);
    Task<(InventCountGroupRecordDto? Group, string? Error)> UpdateAsync(long recId, InventCountGroupInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}
