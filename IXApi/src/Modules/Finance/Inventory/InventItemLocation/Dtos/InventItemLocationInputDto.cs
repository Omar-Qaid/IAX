namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventItemLocationInputDto(string ItemId, string InventDimId,
    decimal PickingLocationMaxQty, decimal PickingLocationRefillMin, string? CountGroupId = null);
