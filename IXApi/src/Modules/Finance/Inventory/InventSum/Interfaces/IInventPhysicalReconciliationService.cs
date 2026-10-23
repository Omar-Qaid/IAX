namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventPhysicalReconciliationService
{
    Task<InventPhysicalReconciliationDto?> CheckAsync(string? itemId, string? siteId,
        string? warehouseId, CancellationToken ct = default);
}
