using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record EcoResProductMappingSource(
    EcoResProduct Product,
    EcoResProductTranslation? English,
    EcoResProductTranslation? Arabic);

public sealed class EcoResProductMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EcoResProductMappingSource, EcoResProductDto>()
            .Map(d => d.Id, s => s.Product.RecId.ToString())
            .Map(d => d.ProductNumber, s => s.Product.DisplayProductNumber)
            .Map(d => d.Name, s => s.English != null ? s.English.Name : string.Empty)
            .Map(d => d.NameAlias, s => s.Arabic != null ? s.Arabic.Name : string.Empty)
            .Map(d => d.Description, s => s.English != null ? s.English.Description : string.Empty)
            .Map(d => d.SearchName, s => s.Product.SearchName)
            .Map(d => d.ProductType, s => s.Product.ProductType.ToString())
            .Map(d => d.ProductSubtype, _ => "Product");
    }
}
