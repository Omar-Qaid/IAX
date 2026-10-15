using System.Globalization;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record ReleasedProductMappingSource(
    long ItemRecId,
    string ItemNumber,
    string Name,
    string NameAliasLocalized,
    string NameAlias,
    string ProductType,
    string ProductSubtype,
    decimal SalesPrice,
    decimal PurchasePrice,
    decimal CostPrice = 0,
    decimal SalesPriceQuantity = 1,
    decimal PurchasePriceQuantity = 1,
    string SalesTaxItemGroupId = "",
    string PurchaseTaxItemGroupId = "",
    string ItemGroupId = "",
    string ModelGroupId = "",
    string StorageDimensionGroupId = "",
    string TrackingDimensionGroupId = "",
    string InventoryUnit = "",
    string PurchaseUnit = "",
    string SalesUnit = "",
    decimal PurchaseToInventoryFactor = 0,
    decimal SalesToInventoryFactor = 0,
    ReleasedProductDefaultOrderSettingsDto? DefaultOrderSettings = null);

public sealed class ReleasedProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ReleasedProductMappingSource, ReleasedProductListDto>()
            .Map(destination => destination.Id,
                source => source.ItemRecId.ToString(CultureInfo.InvariantCulture));
    }
}
