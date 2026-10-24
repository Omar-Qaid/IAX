namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventPackagingService
{
    Task<IReadOnlyList<InventPackagingGroupRecordDto>> GroupsAsync(CancellationToken ct);
    Task<IReadOnlyList<InventPackagingUnitRecordDto>> UnitsAsync(CancellationToken ct);
    Task<(InventPackagingGroupRecordDto? Group, string? Error)> CreateGroupAsync(InventPackagingGroupInputDto input, CancellationToken ct);
    Task<(InventPackagingGroupRecordDto? Group, string? Error)> UpdateGroupAsync(long recId, InventPackagingGroupInputDto input, CancellationToken ct);
    Task<string?> DeleteGroupAsync(long recId, CancellationToken ct);
}
