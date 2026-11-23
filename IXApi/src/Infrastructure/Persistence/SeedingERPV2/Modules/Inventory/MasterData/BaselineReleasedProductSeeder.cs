using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>
/// One stocked product, shared and released: translations, the inventory, purchase and sales modules, its item,
/// model, storage and tracking groups, its default site and warehouse, and its order settings.
/// </summary>
public sealed class BaselineReleasedProductSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Inventory.ReleasedProduct";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineInventorySetupSeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var item = InventoryBaselineCatalog.Load().Product;
        var product = await c.AddMissingAsync<EcoResProduct>(x => x.DisplayProductNumber == item.ItemId,
            new() { DisplayProductNumber = item.ItemId, SearchName = item.Name, ProductType = ItemType.Item }, ct);
        await c.AddMissingAsync<EcoResDistinctProduct>(x => x.RecId == product.RecId, new() { RecId = product.RecId }, ct);
        foreach (var translation in item.Translations)
            await c.AddMissingAsync<EcoResProductTranslation>(x => x.Product == product.RecId && x.LanguageId == translation.Language,
                new() { Product = product.RecId, LanguageId = translation.Language, Name = translation.Name, Description = translation.Description }, ct);

        await c.AddMissingAsync<InventTable>(x => x.ItemId == item.ItemId, new()
        {
            ItemId = item.ItemId, Product = product.RecId, Name = item.Name, NameAlias = item.NameAlias, ItemType = ItemType.Item, BomUnitId = item.Unit
        }, ct);
        foreach (var (module, price, tax) in new[]
                 {
                     (ModuleInventPurchSales.Inventory, 0m, string.Empty),
                     (ModuleInventPurchSales.Purchase, item.PurchasePrice, item.TaxItemGroup),
                     (ModuleInventPurchSales.Sales, item.SalesPrice, item.TaxItemGroup)
                 })
            await c.AddMissingAsync<InventTableModule>(x => x.ItemId == item.ItemId && x.ModuleType == module, new()
            {
                ItemId = item.ItemId, ModuleType = module, UnitId = item.Unit, Price = price, PriceUnit = 1, PriceDate = DateTime.UtcNow, TaxItemGroupId = tax
            }, ct);

        // Group assignments carry the item's company on their own key columns.
        await c.AddMissingAsync<InventItemGroupItem>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId,
            new() { ItemId = item.ItemId, ItemGroupId = item.ItemGroup, ItemDataAreaId = c.Company, ItemGroupDataAreaId = c.Company }, ct);
        await c.AddMissingAsync<InventModelGroupItem>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId,
            new() { ItemId = item.ItemId, ModelGroupId = item.ModelGroup, ItemDataAreaId = c.Company, ModelGroupDataAreaId = c.Company }, ct);
        var storage = await c.Rows<EcoResStorageDimensionGroup>().Where(x => x.Name == item.StorageGroup).Select(x => x.RecId).SingleAsync(ct);
        await c.AddMissingAsync<EcoResStorageDimensionGroupItem>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId,
            new() { ItemId = item.ItemId, ItemDataAreaId = c.Company, StorageDimensionGroup = storage }, ct);
        var tracking = await c.Rows<EcoResTrackingDimensionGroup>().Where(x => x.Name == item.TrackingGroup).Select(x => x.RecId).SingleAsync(ct);
        await c.AddMissingAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId,
            new() { ItemId = item.ItemId, ItemDataAreaId = c.Company, TrackingDimensionGroup = tracking }, ct);

        await c.AddMissingAsync<InventDim>(x => x.InventDimId == item.DefaultDimension,
            new() { InventDimId = item.DefaultDimension, InventSiteId = item.Site, InventLocationId = item.Warehouse }, ct);
        await c.AddMissingAsync<InventItemPurchSetup>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId && x.InventDimId == string.Empty, new()
        {
            ItemId = item.ItemId, ItemDataAreaId = c.Company, InventDimId = string.Empty, InventDimIdDefault = item.DefaultDimension,
            LowestQty = 1, StandardQty = 10, MultipleQty = 1, LeadTime = 5, Stopped = 0
        }, ct);
        await c.AddMissingAsync<InventItemSalesSetup>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId && x.InventDimId == string.Empty, new()
        {
            ItemId = item.ItemId, ItemDataAreaId = c.Company, InventDimId = string.Empty, InventDimIdDefault = item.DefaultDimension,
            LowestQty = 1, StandardQty = 1, MultipleQty = 1, LeadTime = 0, Stopped = 0
        }, ct);
        await c.AddMissingAsync<InventItemInventSetup>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId && x.InventDimId == string.Empty, new()
        {
            ItemId = item.ItemId, ItemDataAreaId = c.Company, InventDimId = string.Empty, InventDimIdDefault = item.DefaultDimension,
            LowestQty = 1, StandardQty = 1, MultipleQty = 1, Stopped = 0
        }, ct);
        await c.AddMissingAsync<InventItemSetupSupplyType>(x => x.ItemDataAreaId == c.Company && x.ItemId == item.ItemId,
            new() { ItemId = item.ItemId, ItemDataAreaId = c.Company, DefaultOrderType = 1 }, ct);
    }
}
