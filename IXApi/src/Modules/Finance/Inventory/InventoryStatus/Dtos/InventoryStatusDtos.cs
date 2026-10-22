namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;

public sealed record InventoryStatusInputDto(string InventStatusId, string Name, bool Blocked);
public sealed record InventoryStatusDto(long RecId, string InventStatusId, string Name, bool Blocked);
