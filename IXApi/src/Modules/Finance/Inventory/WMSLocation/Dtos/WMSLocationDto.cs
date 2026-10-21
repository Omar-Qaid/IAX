namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class WMSLocationDto
{
    public long RecId { get; set; }
    public string InventLocationId { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string WMSLocationId { get; set; } = string.Empty;
    public string InputLocation { get; set; } = string.Empty;
    public string CheckText { get; set; } = string.Empty;
    public int LocationType { get; set; }
    public int SortCode { get; set; }
}

public sealed class WMSLocationInputDto
{
    public string InventLocationId { get; set; } = string.Empty;
    public string WMSLocationId { get; set; } = string.Empty;
    public string InputLocation { get; set; } = string.Empty;
    public string CheckText { get; set; } = string.Empty;
    public int LocationType { get; set; }
    public int SortCode { get; set; }
}
