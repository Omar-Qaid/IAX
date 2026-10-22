namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

public sealed record ProductLifecycleStateInputDto(string StateId, string Description,
    bool IsDefaultOnProductRelease, bool BlockSales = false, bool BlockPurchase = false,
    bool BlockInventory = false);
public sealed record ProductLifecycleStateDto(long RecId, string StateId, string Description,
    bool IsDefaultOnProductRelease, bool BlockSales, bool BlockPurchase,
    bool BlockInventory);
public sealed record ReleasedProductLifecycleDto(long RecId, string ItemId, string ProductLifecycleStateId);
public sealed record ReleasedProductStatusInputDto(string StateId);
