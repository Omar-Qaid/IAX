namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventSiteDto
{
    public string Id { get; set; } = string.Empty;
    public long RecId { get; set; }
    public string SiteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DefaultInventStatusId { get; set; } = string.Empty;
    public int TimeZone { get; set; }
    public bool IsReceivingWarehouseOverrideAllowed { get; set; }
    public long DefaultDimension { get; set; }
    public List<InventSiteWarehouseDto> Warehouses { get; set; } = [];
}

public sealed class InventSiteWarehouseDto
{
    public string Id { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string InventSiteId { get; set; } = string.Empty;
}

