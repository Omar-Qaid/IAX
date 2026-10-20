using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Seeds a repeatable, end-to-end D365-style inventory setup example.
/// This is master/setup data only; it creates no stock movement or accounting voucher.
/// </summary>
public sealed class InventorySeeder : ISeeder
{
    private const string Company = "dat";
    private const string ProductNumber = "ITEM-001";
    private const string ItemGroup = "FURNITURE";
    private const string ModelGroup = "FIFO";
    private const string StorageGroup = "SW";
    private const string TrackingGroup = "NONE";
    private const string Unit = "PCS";
    private const string DefaultDimension = "ITEM001-JED-1";

    public async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users,
        CancellationToken ct)
    {
        await SeedSetupGroupsAsync(db, ct);
        var productId = await SeedSharedProductAsync(db, ct);
        await SeedReleasedProductAsync(db, productId, ct);
        await SeedPostingRulesAsync(db, ct);
    }

    private static async Task SeedSetupGroupsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        if (!await db.Set<InventItemGroup>().IgnoreQueryFilters().AnyAsync(
                x => x.DataAreaId == Company && x.ItemGroupId == ItemGroup, ct))
        {
            db.Add(new InventItemGroup
            {
                DataAreaId = Company,
                ItemGroupId = ItemGroup,
                Name = "Furniture",
                TaxItemGroupIdSales = "VAT15",
                TaxItemGroupIdPurch = "VAT15"
            });
        }

        if (!await db.Set<InventModelGroup>().IgnoreQueryFilters().AnyAsync(
                x => x.DataAreaId == Company && x.ModelGroupId == ModelGroup, ct))
        {
            db.Add(new InventModelGroup
            {
                DataAreaId = Company,
                ModelGroupId = ModelGroup,
                Name = "FIFO stocked items",
                InventModel = InventModel.FIFO,
                StockedProduct = NoYes.Yes,
                PostOnHandPhysical = NoYes.Yes,
                PostOnHandFinancial = NoYes.Yes,
                MandatoryReceive = NoYes.Yes,
                NegativeFinancial = NoYes.Yes,
                PdsDispositionCode = string.Empty
            });
        }

        var storageGroup = await db.Set<EcoResStorageDimensionGroup>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == StorageGroup, ct);
        if (storageGroup is null)
        {
            storageGroup = new EcoResStorageDimensionGroup
            {
                Name = StorageGroup,
                Description = "Site and warehouse"
            };
            db.Add(storageGroup);
            await db.SaveChangesAsync(ct);
        }
        if (!await db.Set<EcoResStorageDimensionGroupFldSetup>().IgnoreQueryFilters().AnyAsync(x => x.StorageDimensionGroup == storageGroup.RecId, ct))
            db.AddRange(
                new EcoResStorageDimensionGroupFldSetup { StorageDimensionGroup = storageGroup.RecId, DimensionFieldId = InventDimensionFields.Site, LineNumber = 1, IsActive = true },
                new EcoResStorageDimensionGroupFldSetup { StorageDimensionGroup = storageGroup.RecId, DimensionFieldId = InventDimensionFields.Warehouse, LineNumber = 2, IsActive = true },
                new EcoResStorageDimensionGroupFldSetup { StorageDimensionGroup = storageGroup.RecId, DimensionFieldId = InventDimensionFields.Location, LineNumber = 3 });

        var trackingGroup = await db.Set<EcoResTrackingDimensionGroup>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == TrackingGroup, ct);
        if (trackingGroup is null)
        {
            trackingGroup = new EcoResTrackingDimensionGroup
            {
                Name = TrackingGroup,
                Description = "No tracking dimensions"
            };
            db.Add(trackingGroup);
            await db.SaveChangesAsync(ct);
        }
        if (!await db.Set<EcoResTrackingDimensionGroupFldSetup>().IgnoreQueryFilters().AnyAsync(x => x.TrackingDimensionGroup == trackingGroup.RecId, ct))
            db.AddRange(
                new EcoResTrackingDimensionGroupFldSetup { TrackingDimensionGroup = trackingGroup.RecId, DimensionFieldId = InventDimensionFields.BatchNumber, LineNumber = 1 },
                new EcoResTrackingDimensionGroupFldSetup { TrackingDimensionGroup = trackingGroup.RecId, DimensionFieldId = InventDimensionFields.SerialNumber, LineNumber = 2 });

        await db.SaveChangesAsync(ct);
    }

    private static async Task<long> SeedSharedProductAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var product = await db.Set<EcoResProduct>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DisplayProductNumber == ProductNumber, ct);

        if (product is null)
        {
            product = new EcoResProduct
            {
                DisplayProductNumber = ProductNumber,
                SearchName = "Office Chair",
                ProductType = ItemType.Item
            };
            db.Add(product);
            await db.SaveChangesAsync(ct);
        }

        if (!await db.Set<EcoResDistinctProduct>().IgnoreQueryFilters().AnyAsync(x => x.RecId == product.RecId, ct))
            db.Add(new EcoResDistinctProduct { RecId = product.RecId });

        await AddTranslationAsync(db, product.RecId, "en-us", "Office Chair", "Basic stocked office chair", ct);
        await AddTranslationAsync(db, product.RecId, "ar", "منشفة حمام", "منشفة حمام قياسية", ct);
        await db.SaveChangesAsync(ct);
        return product.RecId;
    }

    private static async Task AddTranslationAsync(
        ApplicationDbContext db, long productId, string language, string name, string description, CancellationToken ct)
    {
        if (!await db.Set<EcoResProductTranslation>().IgnoreQueryFilters().AnyAsync(
                x => x.Product == productId && x.LanguageId == language, ct))
        {
            db.Add(new EcoResProductTranslation
            {
                Product = productId,
                LanguageId = language,
                Name = name,
                Description = description
            });
        }
    }

    private static async Task SeedReleasedProductAsync(ApplicationDbContext db, long productId, CancellationToken ct)
    {
        if (!await db.Set<InventTable>().IgnoreQueryFilters().AnyAsync(
                x => x.DataAreaId == Company && x.ItemId == ProductNumber, ct))
        {
            db.Add(new InventTable
            {
                DataAreaId = Company,
                ItemId = ProductNumber,
                Product = productId,
                Name = "Office Chair",
                NameAlias = "منشفة حمام",
                ItemType = ItemType.Item,
                BomUnitId = Unit
            });
        }

        await AddModuleAsync(db, ModuleInventPurchSales.Inventory, 0m, string.Empty, ct);
        await AddModuleAsync(db, ModuleInventPurchSales.Purchase, 100m, "VAT15", ct);
        await AddModuleAsync(db, ModuleInventPurchSales.Sales, 150m, "VAT15", ct);

        if (!await db.Set<InventItemGroupItem>().IgnoreQueryFilters().AnyAsync(
                x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber, ct))
            db.Add(new InventItemGroupItem { ItemId = ProductNumber, ItemGroupId = ItemGroup, ItemDataAreaId = Company, ItemGroupDataAreaId = Company });

        if (!await db.Set<InventModelGroupItem>().IgnoreQueryFilters().AnyAsync(
                x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber, ct))
            db.Add(new InventModelGroupItem { ItemId = ProductNumber, ModelGroupId = ModelGroup, ItemDataAreaId = Company, ModelGroupDataAreaId = Company });

        if (!await db.Set<EcoResStorageDimensionGroupItem>().IgnoreQueryFilters().AnyAsync(
                x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber, ct))
            db.Add(new EcoResStorageDimensionGroupItem { ItemId = ProductNumber, ItemDataAreaId = Company,
                StorageDimensionGroup = await db.Set<EcoResStorageDimensionGroup>().IgnoreQueryFilters().Where(x => x.Name == StorageGroup).Select(x => x.RecId).SingleAsync(ct) });

        if (!await db.Set<EcoResTrackingDimensionGroupItem>().IgnoreQueryFilters().AnyAsync(
                x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber, ct))
            db.Add(new EcoResTrackingDimensionGroupItem { ItemId = ProductNumber, ItemDataAreaId = Company,
                TrackingDimensionGroup = await db.Set<EcoResTrackingDimensionGroup>().IgnoreQueryFilters().Where(x => x.Name == TrackingGroup).Select(x => x.RecId).SingleAsync(ct) });

        await EnsureDefaultDimensionAsync(db, ct);
        await AddOrderSetupAsync<InventItemPurchSetup>(db, () => new InventItemPurchSetup
        {
            ItemId = ProductNumber, ItemDataAreaId = Company, InventDimId = string.Empty,
            InventDimIdDefault = DefaultDimension, LowestQty = 1, StandardQty = 10, MultipleQty = 1, LeadTime = 5, Stopped = 0
        }, ct);
        await AddOrderSetupAsync<InventItemSalesSetup>(db, () => new InventItemSalesSetup
        {
            ItemId = ProductNumber, ItemDataAreaId = Company, InventDimId = string.Empty,
            InventDimIdDefault = DefaultDimension, LowestQty = 1, StandardQty = 1, MultipleQty = 1, LeadTime = 0, Stopped = 0
        }, ct);
        await AddOrderSetupAsync<InventItemInventSetup>(db, () => new InventItemInventSetup
        {
            ItemId = ProductNumber, ItemDataAreaId = Company, InventDimId = string.Empty,
            InventDimIdDefault = DefaultDimension, LowestQty = 1, StandardQty = 1, MultipleQty = 1, Stopped = 0
        }, ct);

        if (!await db.Set<InventItemSetupSupplyType>().IgnoreQueryFilters().AnyAsync(
                x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber, ct))
            db.Add(new InventItemSetupSupplyType { ItemId = ProductNumber, ItemDataAreaId = Company, DefaultOrderType = 1 });

        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureDefaultDimensionAsync(ApplicationDbContext db, CancellationToken ct)
    {
        if (await db.Set<InventDim>().IgnoreQueryFilters().AnyAsync(
                x => x.DataAreaId == Company && x.InventDimId == DefaultDimension, ct)) return;

        db.Add(new InventDim
        {
            DataAreaId = Company,
            InventDimId = DefaultDimension,
            InventSiteId = "JED",
            InventLocationId = "JED-1"
        });
        await db.SaveChangesAsync(ct);
    }

    private static async Task AddModuleAsync(
        ApplicationDbContext db, ModuleInventPurchSales type, decimal price, string taxGroup, CancellationToken ct)
    {
        if (!await db.Set<InventTableModule>().IgnoreQueryFilters().AnyAsync(
                x => x.DataAreaId == Company && x.ItemId == ProductNumber && x.ModuleType == type, ct))
        {
            db.Add(new InventTableModule
            {
                DataAreaId = Company,
                ItemId = ProductNumber,
                ModuleType = type,
                UnitId = Unit,
                Price = price,
                PriceUnit = 1,
                PriceDate = DateTime.UtcNow,
                TaxItemGroupId = taxGroup
            });
        }
    }

    private static async Task AddOrderSetupAsync<T>(ApplicationDbContext db, Func<T> create, CancellationToken ct)
        where T : class
    {
        var exists = typeof(T) switch
        {
            var type when type == typeof(InventItemPurchSetup) => await db.Set<InventItemPurchSetup>().IgnoreQueryFilters().AnyAsync(x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber && x.InventDimId == string.Empty, ct),
            var type when type == typeof(InventItemSalesSetup) => await db.Set<InventItemSalesSetup>().IgnoreQueryFilters().AnyAsync(x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber && x.InventDimId == string.Empty, ct),
            _ => await db.Set<InventItemInventSetup>().IgnoreQueryFilters().AnyAsync(x => x.ItemDataAreaId == Company && x.ItemId == ProductNumber && x.InventDimId == string.Empty, ct)
        };
        if (!exists) db.Add(create());
    }

    private static async Task SeedPostingRulesAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var accountIds = new[] { "50010000", "40010000" };
        var accounts = await db.Set<MainAccount>().IgnoreQueryFilters()
            .Where(x => accountIds.Contains(x.MainAccountId))
            .ToDictionaryAsync(x => x.MainAccountId, ct);
        if (accounts.Count != accountIds.Length) return;

        foreach (var account in accounts.Values)
        {
            if (!await db.Set<DimensionAttributeValueCombination>().IgnoreQueryFilters().AnyAsync(
                    x => x.DataAreaId == Company && x.MainAccount == account.RecId, ct))
            {
                db.Add(new DimensionAttributeValueCombination
                {
                    DataAreaId = Company,
                    DisplayValue = account.MainAccountId,
                    MainAccount = account.RecId,
                    MainAccountValue = account.MainAccountId
                });
            }
        }
        await db.SaveChangesAsync(ct);

        var dimensions = await db.Set<DimensionAttributeValueCombination>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == Company && accountIds.Contains(x.MainAccountValue))
            .ToDictionaryAsync(x => x.MainAccountValue, x => x.RecId, ct);

        await AddPostingAsync(db, InventAccountType.PurchPackingSlip, dimensions["50010000"], ct);
        await AddPostingAsync(db, InventAccountType.SalesPackingSlip, dimensions["50010000"], ct);
        await AddPostingAsync(db, InventAccountType.SalesRevenue, dimensions["40010000"], ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task AddPostingAsync(
        ApplicationDbContext db, InventAccountType type, long ledgerDimension, CancellationToken ct)
    {
        if (!await db.Set<InventPosting>().IgnoreQueryFilters().AnyAsync(
                x => x.DataAreaId == Company && x.InventAccountType == type &&
                     x.ItemCode == TableGroupAll.All && x.CustVendCode == TableGroupAll.All, ct))
        {
            db.Add(new InventPosting
            {
                DataAreaId = Company,
                InventAccountType = type,
                ItemCode = TableGroupAll.All,
                ItemRelation = string.Empty,
                CustVendCode = TableGroupAll.All,
                CustVendRelation = string.Empty,
                LedgerDimension = ledgerDimension,
                TaxGroupId = string.Empty
            });
        }
    }
}
