using System.Text.Json.Serialization;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventSiteLookupDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

public sealed class InventWarehouseLookupDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    [JsonPropertyName("siteId")]
    public string SiteId { get; init; } = string.Empty;
}

public sealed class InventLocationLookupDto
{
    [JsonPropertyName("sites")]
    public List<InventSiteLookupDto> Sites { get; init; } = [];
    [JsonPropertyName("warehouses")]
    public List<InventWarehouseLookupDto> Warehouses { get; init; } = [];
}
