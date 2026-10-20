namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventParametersDto
{
    public long RecId { get; set; }
    public string DefaultUnitId { get; set; } = string.Empty;
    public string MultiSiteDefaultInventSiteId { get; set; } = string.Empty;
    public string MultiSiteDefaultInventLocationId { get; set; } = string.Empty;
    public string MovementJournalNameId { get; set; } = string.Empty;
    public string LossProfitJournalNameId { get; set; } = string.Empty;
    public string TransferJournalNameId { get; set; } = string.Empty;
    public string CountJournalNameId { get; set; } = string.Empty;
    public bool ReserveOnOrdered { get; set; }
    public bool MandatoryItemTaxGroup { get; set; }
}

public sealed class InventParametersLookupsDto
{
    public List<InventParametersLookupDto> Units { get; set; } = [];
    public List<InventParametersLookupDto> Sites { get; set; } = [];
    public List<InventParametersWarehouseLookupDto> Warehouses { get; set; } = [];
    public List<InventParametersJournalLookupDto> JournalNames { get; set; } = [];
}

public sealed record InventParametersLookupDto(string Id, string Name);
public sealed record InventParametersWarehouseLookupDto(string Id, string Name, string SiteId);
public sealed record InventParametersJournalLookupDto(string Id, string Name, int JournalType);
