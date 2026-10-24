namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record FifoCostAllocationDto(long ReceiptRecId, decimal Quantity, decimal UnitCost, decimal Cost);
