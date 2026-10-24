namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventBatchService
{
    Task<IReadOnlyList<InventBatchDto>> ListAsync(string? itemId, CancellationToken ct);
    Task<InventBatchDto?> GetAsync(long recId, CancellationToken ct);
    Task<(InventBatchDto? Batch, string? Error)> CreateAsync(InventBatchInputDto input, CancellationToken ct);
    Task<(InventBatchDto? Batch, string? Error)> UpdateAsync(long recId, InventBatchInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}
