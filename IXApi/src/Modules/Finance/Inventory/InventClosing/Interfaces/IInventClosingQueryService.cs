namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventClosingQueryService
{
    Task<IReadOnlyList<InventClosingRecordDto>> ListAsync(CancellationToken ct);
    Task<InventClosingRecordDto?> GetAsync(long recId, CancellationToken ct);
}
