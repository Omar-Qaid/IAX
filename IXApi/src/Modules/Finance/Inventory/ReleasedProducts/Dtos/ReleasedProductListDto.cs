namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ReleasedProductListDto
{
    public string Id { get; init; } = string.Empty;
    public string ItemNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameAliasLocalized { get; init; } = string.Empty;
    public string NameAlias { get; init; } = string.Empty;
    public string ProductType { get; init; } = string.Empty;
    public string ProductSubtype { get; init; } = string.Empty;
    public decimal SalesPrice { get; init; }
    public decimal PurchasePrice { get; init; }
    public decimal SalesPriceQuantity { get; init; } = 1;
    public decimal PurchasePriceQuantity { get; init; } = 1;
    public string SalesTaxItemGroupId { get; init; } = string.Empty;
    public string PurchaseTaxItemGroupId { get; init; } = string.Empty;
    public decimal CostPrice { get; init; }
    public string ItemGroupId { get; init; } = string.Empty;
    public string ModelGroupId { get; init; } = string.Empty;
    public string StorageDimensionGroupId { get; init; } = string.Empty;
    public string TrackingDimensionGroupId { get; init; } = string.Empty;
    public string InventoryUnit { get; init; } = string.Empty;
    public string PurchaseUnit { get; init; } = string.Empty;
    public string SalesUnit { get; init; } = string.Empty;
    public decimal PurchaseToInventoryFactor { get; init; }
    public decimal SalesToInventoryFactor { get; init; }
    public ReleasedProductDefaultOrderSettingsDto DefaultOrderSettings { get; init; } = new();
}

public sealed class ReleasedProductGroupAssignmentDto
{
    public string ItemGroupId { get; init; } = string.Empty;
    public string ModelGroupId { get; init; } = string.Empty;
    public string StorageDimensionGroupId { get; init; } = string.Empty;
    public string TrackingDimensionGroupId { get; init; } = string.Empty;
}

public sealed class ReleasedProductUnitAssignmentDto
{
    public string InventoryUnit { get; init; } = string.Empty;
    public string PurchaseUnit { get; init; } = string.Empty;
    public string SalesUnit { get; init; } = string.Empty;
    public ReleasedProductGroupAssignmentDto? Groups { get; init; }
    public decimal PurchaseToInventoryFactor { get; init; }
    public decimal SalesToInventoryFactor { get; init; }
    public ReleasedProductCommercialAssignmentDto? Commercial { get; init; }
    public ReleasedProductDefaultOrderSettingsDto? DefaultOrderSettings { get; init; }
}

public sealed class ReleasedProductCommercialAssignmentDto
{
    public decimal PurchasePrice { get; init; }
    public decimal PurchasePriceQuantity { get; init; } = 1;
    public string PurchaseTaxItemGroupId { get; init; } = string.Empty;
    public decimal SalesPrice { get; init; }
    public decimal SalesPriceQuantity { get; init; } = 1;
    public string SalesTaxItemGroupId { get; init; } = string.Empty;
}

public sealed class CreateSharedProductDto
{
    public string ProductNumber { get; init; } = string.Empty;
    public string SearchName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string LanguageId { get; init; } = "en-us";
}

public sealed class ReleaseProductDto
{
    public string ProductId { get; init; } = string.Empty;
    public string ItemNumber { get; init; } = string.Empty;
    public string NameAlias { get; init; } = string.Empty;
}

public sealed class ConfigureReleasedProductDto
{
    public string InventoryUnit { get; init; } = string.Empty;
    public string PurchaseUnit { get; init; } = string.Empty;
    public string SalesUnit { get; init; } = string.Empty;
    public decimal PurchaseToInventoryFactor { get; init; } = 1;
    public decimal SalesToInventoryFactor { get; init; } = 1;
    public decimal InventoryPrice { get; init; }
    public decimal PurchasePrice { get; init; }
    public decimal SalesPrice { get; init; }
    public decimal PriceUnit { get; init; } = 1;
    public string PurchaseTaxItemGroupId { get; init; } = string.Empty;
    public string SalesTaxItemGroupId { get; init; } = string.Empty;
    public string ItemGroupId { get; init; } = string.Empty;
    public string ModelGroupId { get; init; } = string.Empty;
    public string StorageDimensionGroupId { get; init; } = string.Empty;
    public string TrackingDimensionGroupId { get; init; } = string.Empty;
    public ReleasedProductDefaultOrderSettingsDto? DefaultOrderSettings { get; init; }
}

public sealed class ReleasedProductDefaultOrderSettingsDto
{
    public int DefaultOrderType { get; init; } = 1;
    public ReleasedProductOrderModuleSettingsDto Purchase { get; init; } = new();
    public ReleasedProductOrderModuleSettingsDto Sales { get; init; } = new();
    public ReleasedProductOrderModuleSettingsDto Inventory { get; init; } = new();
}

public sealed class ReleasedProductOrderModuleSettingsDto
{
    public string SiteId { get; init; } = string.Empty;
    public string WarehouseId { get; init; } = string.Empty;
    public decimal StandardQuantity { get; init; }
    public decimal MinimumQuantity { get; init; }
    public decimal MaximumQuantity { get; init; }
    public decimal MultipleQuantity { get; init; }
    public int LeadTimeDays { get; init; }
    public bool Stopped { get; init; }
}

public sealed class ReleasedProductValidationDto
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
}

public sealed class CreateReleasedProductDto
{
    public string ProductNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameAlias { get; init; } = string.Empty;
    public string SearchName { get; init; } = string.Empty;
    public string ItemNumber { get; init; } = string.Empty;
    public string ItemNameAlias { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string BuyerGroupId { get; init; } = string.Empty;
    public string VendorAccount { get; init; } = string.Empty;
    public string BomUnit { get; init; } = string.Empty;
    public ConfigureReleasedProductDto? Configuration { get; init; }
}

public sealed class ReleasedProductLookupDto
{
    public List<ReleasedProductLookupOptionDto> Units { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> ItemGroups { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> ModelGroups { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> StorageDimensionGroups { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> TrackingDimensionGroups { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> TaxItemGroups { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> Vendors { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> Sites { get; init; } = [];
    public List<ReleasedProductLookupOptionDto> Warehouses { get; init; } = [];
}

public sealed record ReleasedProductLookupOptionDto(string Value, string Label);
