using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>Inventory-owned fixtures, isolated from AP/AR example stock and prior fiscal years.</summary>
public sealed class InventoryCycleMasterSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventoryCycleMasterSeeder";
    public IReadOnlyCollection<string> Dependencies =>
    ["ERPV2.InventTableModuleSeeder", "ERPV2.EcoResStorageDimensionGroupFldSetupSeeder",
     "ERPV2.EcoResTrackingDimensionGroupFldSetupSeeder", "ERPV2.InventDimSeeder", "ERPV2.InventoryExampleTaxSeeder"];

    public static string StockItem(int year) => $"IV2-{year}-STOCK";
    public static string ServiceItem(int year) => $"IV2-{year}-SVC";
    public static string Item(int year, string suffix) => $"IV2-{year}-{suffix}";

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var storage = await c.RequireAsync<EcoResStorageDimensionGroup>(x => x.Name == $"APV2-{c.Company}-STORE", ct);
        var tracking = await c.RequireAsync<EcoResTrackingDimensionGroup>(x => x.Name == $"APV2-{c.Company}-NONE", ct);
        await c.RequireAsync<InventDim>(x => x.InventDimId == "APV2-DIM", ct);
        foreach (var suffix in InventoryCycleSeedExample.ItemSuffixes)
        {
            var service = suffix == InventoryCycleSeedExample.ServiceSuffix;
            var itemId = Item(c.FiscalYear, suffix);
            var productNumber = $"{c.Company}-{itemId}";
            var product = await c.EnsureAsync<EcoResProduct>(x => x.DisplayProductNumber == productNumber,
                new() { DisplayProductNumber = productNumber, SearchName = itemId,
                    ProductType = InventoryCycleSeedExample.ProductType(suffix) }, ct);
            await c.EnsureAsync<InventTable>(x => x.ItemId == itemId,
                new() { ItemId = itemId, Product = product.RecId, Name = InventoryCycleSeedExample.Name(suffix),
                    NameAlias = itemId, ItemType = InventoryCycleSeedExample.ProductType(suffix),
                    PrimaryVendorID = $"APV2-{c.Company}-LOCAL", MatchingPolicy = service ? MatchingPolicy.TwoWay : MatchingPolicy.ThreeWay }, ct);
            await c.EnsureAsync<InventItemGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company,
                new() { ItemId = itemId, ItemDataAreaId = c.Company, ItemGroupId = InventoryCycleSeedExample.ItemGroup,
                    ItemGroupDataAreaId = c.Company }, ct);
            await c.EnsureAsync<InventModelGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company,
                new() { ItemId = itemId, ItemDataAreaId = c.Company, ModelGroupId = InventoryCycleSeedExample.ModelGroup(suffix),
                    ModelGroupDataAreaId = c.Company }, ct);
            foreach (var module in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
                await c.EnsureAsync<InventTableModule>(x => x.ItemId == itemId && x.ModuleType == module,
                    new() { ItemId = itemId, ModuleType = module, UnitId = InventoryCycleSeedExample.Unit,
                        PriceUnit = InventoryCycleSeedExample.PriceUnit, PriceQty = 1,
                        Price = InventoryCycleSeedExample.Price(suffix, module),
                        TaxItemGroupId = InventoryCycleSeedExample.ItemTaxGroup }, ct);
            if (service) continue;
            await c.EnsureAsync<EcoResStorageDimensionGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company,
                new() { ItemId = itemId, ItemDataAreaId = c.Company, StorageDimensionGroup = storage.RecId }, ct);
            await c.EnsureAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company,
                new() { ItemId = itemId, ItemDataAreaId = c.Company, TrackingDimensionGroup = tracking.RecId }, ct);
            await c.EnsureAsync<InventItemPurchSetup>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company && x.InventDimId == "",
                new() { ItemId = itemId, ItemDataAreaId = c.Company, InventDimIdDefault = "APV2-DIM", LowestQty = 1, MultipleQty = 1, StandardQty = 100 }, ct);
            await c.EnsureAsync<InventItemInventSetup>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company && x.InventDimId == "",
                new() { ItemId = itemId, ItemDataAreaId = c.Company, InventDimIdDefault = "APV2-DIM", LowestQty = 1, MultipleQty = 1, StandardQty = 1 }, ct);
        }
    }
}
