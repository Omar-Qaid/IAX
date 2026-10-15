namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class StorageDimensionGroupDto
{
    public string Id { get; init; } = string.Empty;
    public string GroupId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool UseSite { get; init; }
    public bool UseWarehouse { get; init; }
    public bool UseLocation { get; init; }
}

public sealed class StorageDimensionGroupInputDto
{
    public string GroupId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool UseSite { get; init; }
    public bool UseWarehouse { get; init; }
    public bool UseLocation { get; init; }
}
