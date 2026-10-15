namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class TrackingDimensionGroupDto
{
    public string Id { get; init; } = string.Empty;
    public string GroupId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool UseBatchNumber { get; init; }
    public bool UseSerialNumber { get; init; }
}

public sealed class TrackingDimensionGroupInputDto
{
    public string GroupId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool UseBatchNumber { get; init; }
    public bool UseSerialNumber { get; init; }
}
