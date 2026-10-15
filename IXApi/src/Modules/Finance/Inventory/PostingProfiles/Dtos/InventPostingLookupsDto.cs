namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventPostingLookupDto(string Id, string Label);

public sealed class InventPostingLookupsDto
{
    public List<InventPostingLookupDto> Items { get; init; } = [];
    public List<InventPostingLookupDto> ItemGroups { get; init; } = [];
    public List<InventPostingLookupDto> Customers { get; init; } = [];
    public List<InventPostingLookupDto> CustomerGroups { get; init; } = [];
    public List<InventPostingLookupDto> Vendors { get; init; } = [];
    public List<InventPostingLookupDto> VendorGroups { get; init; } = [];
    public List<InventPostingLookupDto> Categories { get; init; } = [];
    public List<InventPostingLookupDto> TaxGroups { get; init; } = [];
    public List<InventPostingLookupDto> LedgerAccounts { get; init; } = [];
}
