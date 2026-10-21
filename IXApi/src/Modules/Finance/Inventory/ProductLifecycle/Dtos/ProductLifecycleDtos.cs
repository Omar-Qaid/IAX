namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

public sealed record ProductLifecycleStateInputDto(string StateId, string Description,
    bool IsDefaultOnProductRelease);
public sealed record ProductLifecycleStateDto(long RecId, string StateId, string Description,
    bool IsDefaultOnProductRelease);
public sealed record ReleasedProductLifecycleDto(long RecId, string ItemId, string ProductLifecycleStateId);
public sealed record ReleasedProductStatusInputDto(string StateId);
