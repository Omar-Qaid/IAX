using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventoryDimensionRecordDto(long RecId, string InventDimId, string SiteId,
    string WarehouseId, string BatchId, string SerialId, string InventoryStatusId);

public sealed record ItemGroupAssignmentRecordDto(long RecId, string ItemId, string ItemGroupId);

public sealed record ItemLocationRecordDto(long RecId, string ItemId, string InventDimId,
    decimal PickingLocationMaxQty, decimal PickingLocationRefillMin, string CountGroupId,
    string DefaultReceiptLocationId, string DefaultIssueLocationId);

public sealed record InventorySettlementRecordDto(long RecId, string ItemId, string InventTransId,
    string SettleTransId, string Voucher, DateTime TransDate, decimal QtySettled,
    decimal CostAmountSettled, decimal CostAmountAdjustment, InventSettleType SettleType,
    bool Posted, bool Cancelled);
