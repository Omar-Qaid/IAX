using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventPackagingUnitRecordDto(long RecId, TableGroupAll ItemCode, string InventDimId,
    string Unit, decimal Factor);
