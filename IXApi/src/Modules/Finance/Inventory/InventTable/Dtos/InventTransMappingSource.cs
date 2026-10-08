using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventTransMappingSource(InventTrans Transaction, InventTransOrigin? Origin, InventDim? Dimension, decimal UnitPrice, decimal CostAmount);
