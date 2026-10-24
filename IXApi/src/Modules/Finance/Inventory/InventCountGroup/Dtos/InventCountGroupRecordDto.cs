using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventCountGroupRecordDto(long RecId, string CountGroupId, string Name,
    InventCountCode CountCode, int CountPeriod);
