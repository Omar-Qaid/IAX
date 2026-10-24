namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IItemGroupAssignmentService
{
    Task<(ItemGroupAssignmentRecordDto? Assignment, string? Error)> AssignAsync(string itemId, ItemGroupAssignmentInputDto input, CancellationToken ct);
}
