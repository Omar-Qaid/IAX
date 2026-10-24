namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventSerialService
{
    Task<IReadOnlyList<InventSerialRecordDto>> ListAsync(string? itemId, CancellationToken ct);
    Task<(InventSerialRecordDto? Serial, string? Error)> CreateAsync(InventSerialInputDto input, CancellationToken ct);
    Task<(InventSerialRecordDto? Serial, string? Error)> UpdateAsync(long recId, InventSerialInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}
