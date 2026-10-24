namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventSerialRecordDto(long RecId, string ItemId, string InventSerialId, DateTime? ProdDate);
