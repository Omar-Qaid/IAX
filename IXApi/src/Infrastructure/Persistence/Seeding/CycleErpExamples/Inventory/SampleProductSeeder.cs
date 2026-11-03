using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class SampleProductSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.Inventory.DemoProducts";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Demo;
        public override IReadOnlyCollection<string> DependsOn => new[]
        {
            "Finance.Inventory.ItemGroups",
            "Finance.Inventory.UnitOfMeasure",
            "Finance.GeneralLedger.Tax"
        };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
            => SeedDataAsync(context.Db, cancellationToken);

        public async Task SeedDataAsync(DbContext db, CancellationToken cancellationToken = default)
        {
            var createdBy = "sys";

            var groupCodes = new[] { "RAW", "FIN", "CONS", "SPR" };
            var groupIdByCode = await db.Set<InventItemGroup>().IgnoreQueryFilters()
                .Where(g => g.DataAreaId == "dat" && groupCodes.Contains(g.ItemGroupId))
                .ToDictionaryAsync(g => g.ItemGroupId, g => g.RecId, cancellationToken);
            var uomIdByCode = await db.Set<UnitOfMeasure>().IgnoreQueryFilters().ToDictionaryAsync(u => u.Symbol, u => u.RecId, cancellationToken);

            var itemSeeds = new[]
            {
                new
                {
                    Code = "ITM-1001", Name = "Bottled Water 500ml", Group = "CONS", Description = "Drinking water, 500 ml bottle",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("BTL", 1m, "6280001000017", false, true, true),
                        ("CTN", 24m, "6280001000024", true, false, false),
                    }
                },
                new
                {
                    Code = "ITM-1002", Name = "A4 Paper Ream", Group = "CONS", Description = "A4 printing paper, 500 sheets per ream",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("PCK", 1m, "6280001000031", false, true, true),
                        ("BOX", 5m, "6280001000048", true, false, false),
                    }
                },
                new
                {
                    Code = "ITM-1003", Name = "Steel Rod 12mm", Group = "RAW", Description = "Reinforcement steel rod, 12 mm diameter",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("M", 1m, "6280001000055", false, true, true),
                        ("M", 1m, "6280001000062", true, false, false),
                    }
                },
                new
                {
                    Code = "ITM-1004", Name = "T-Shirt Cotton", Group = "FIN", Description = "100% cotton T-shirt",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("PCS", 1m, "6280001000079", false, true, true),
                        ("DOZ", 12m, "6280001000086", true, false, false),
                    }
                },
                new
                {
                    Code = "ITM-1005", Name = "Cooking Oil 1L", Group = "FIN", Description = "Vegetable cooking oil, 1 litre bottle",
                    Uoms = new (string Uom, decimal Pack, string? Barcode, bool Buy, bool Sell, bool Inv)[]
                    {
                        ("BTL", 1m, "6280001000093", false, true, true),
                        ("CTN", 12m, "6280001000109", true, false, false),
                    }
                },
            };

            var modulesToAdd = new List<InventTableModule>();

            foreach (var seed in itemSeeds)
            {
                if (!groupIdByCode.TryGetValue(seed.Group, out var groupId)) continue;

                var uomRows = seed.Uoms
                    .Where(u => uomIdByCode.ContainsKey(u.Uom))
                    .ToList();
                if (uomRows.Count == 0) continue;

                var baseRow = uomRows.FirstOrDefault(u => u.Inv);
                if (baseRow == default) continue;
                var sellRow = uomRows.FirstOrDefault(u => u.Sell);
                if (sellRow == default) sellRow = baseRow;
                var buyRow = uomRows.FirstOrDefault(u => u.Buy);
                if (buyRow == default) buyRow = baseRow;

                var item = await db.Set<InventTable>().IgnoreQueryFilters()
                    .SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.ItemId == seed.Code, cancellationToken);
                var product = item == null || item.Product == 0 ? null : await db.Set<EcoResProduct>()
                    .IgnoreQueryFilters().SingleOrDefaultAsync(x => x.RecId == item.Product, cancellationToken);
                product ??= await db.Set<EcoResProduct>().IgnoreQueryFilters()
                    .SingleOrDefaultAsync(x => x.DisplayProductNumber == seed.Code, cancellationToken);
                if (product == null)
                {
                    product = new EcoResProduct { DisplayProductNumber = seed.Code, SearchName = seed.Name, ProductType = ItemType.Item };
                    db.Add(product);
                    await db.SaveChangesAsync(cancellationToken);
                }
                if (!await db.Set<EcoResDistinctProduct>().IgnoreQueryFilters().AnyAsync(x => x.RecId == product.RecId, cancellationToken))
                    db.Add(new EcoResDistinctProduct { RecId = product.RecId });
                if (!await db.Set<EcoResProductTranslation>().IgnoreQueryFilters().AnyAsync(x => x.Product == product.RecId && x.LanguageId == "en-us", cancellationToken))
                    db.Add(new EcoResProductTranslation { Product = product.RecId, LanguageId = "en-us", Name = seed.Name, Description = seed.Description });
                if (item == null)
                {
                    item = new InventTable { ItemId = seed.Code, DataAreaId = "dat", IsActive = true,
                        CreatedBy = createdBy, OwnerAccountId = createdBy, BomUnitId = baseRow.Uom };
                    db.Add(item);
                }
                item.Product = product.RecId;
                if (string.IsNullOrWhiteSpace(item.Name)) item.Name = seed.Name;
                if (!await db.Set<InventItemGroupItem>().IgnoreQueryFilters().AnyAsync(x => x.ItemDataAreaId == "dat" && x.ItemId == seed.Code, cancellationToken))
                    db.Add(new InventItemGroupItem { ItemId = seed.Code, ItemGroupId = seed.Group, ItemDataAreaId = "dat", ItemGroupDataAreaId = "dat" });

                modulesToAdd.Add(new InventTableModule
                {
                    ItemId = seed.Code,
                    ModuleType = ModuleInventPurchSales.Inventory,
                    UnitId = baseRow.Uom,
                    Price = 10m,
                    PriceUnit = 1m,
                    LineDisc = "",
                    TaxItemGroupId = "FULL",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                });

                modulesToAdd.Add(new InventTableModule
                {
                    ItemId = seed.Code,
                    ModuleType = ModuleInventPurchSales.Purchase,
                    UnitId = buyRow.Uom,
                    Price = 12m,
                    PriceUnit = 1m,
                    LineDisc = "",
                    TaxItemGroupId = "FULL",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                });

                modulesToAdd.Add(new InventTableModule
                {
                    ItemId = seed.Code,
                    ModuleType = ModuleInventPurchSales.Sales,
                    UnitId = sellRow.Uom,
                    Price = 15m,
                    PriceUnit = 1m,
                    LineDisc = "",
                    TaxItemGroupId = "FULL",
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy
                });
            }

            var existingModules = await db.Set<InventTableModule>().IgnoreQueryFilters()
                .Where(x => x.DataAreaId == "dat").Select(x => new { x.ItemId, x.ModuleType }).ToListAsync(cancellationToken);
            foreach (var module in modulesToAdd.Where(x => !existingModules.Any(e => e.ItemId == x.ItemId && e.ModuleType == x.ModuleType)))
            {
                module.DataAreaId = "dat";
                db.Add(module);
            }
            await db.SaveChangesAsync(cancellationToken);

            // Packaging conversions
            var packagingSeeds = new (string ItemId, string From, string To, decimal Factor)[]
            {
                ("ITM-1001", "CTN", "BTL", 24m),
                ("ITM-1002", "BOX", "PCK", 5m),
                ("ITM-1004", "DOZ", "PCS", 12m),
                ("ITM-1005", "CTN", "BTL", 12m),
            };
            var packagingItemIds = packagingSeeds.Select(seed => seed.ItemId).ToArray();
            var packagingItems = await db.Set<InventTable>().IgnoreQueryFilters()
                .Where(item => item.DataAreaId == "dat" && packagingItemIds.Contains(item.ItemId))
                .ToDictionaryAsync(item => item.ItemId, cancellationToken);
            var packagingSymbols = packagingSeeds.SelectMany(seed => new[] { seed.From, seed.To }).Distinct().ToArray();
            var packagingUnits = await db.Set<UnitOfMeasure>().IgnoreQueryFilters()
                .Where(unit => packagingSymbols.Contains(unit.Symbol))
                .ToDictionaryAsync(unit => unit.Symbol, cancellationToken);
            var existingPackagingRules = await db.Set<UnitOfMeasureConversion>().IgnoreQueryFilters()
                .Select(rule => new { rule.Product, rule.FromUnitOfMeasure, rule.ToUnitOfMeasure })
                .ToListAsync(cancellationToken);

            foreach (var seed in packagingSeeds)
            {
                if (!packagingItems.TryGetValue(seed.ItemId, out var item)
                    || !packagingUnits.TryGetValue(seed.From, out var from)
                    || !packagingUnits.TryGetValue(seed.To, out var to))
                    continue;

                var productScope = item.Product;
                if (productScope == 0 || existingPackagingRules.Any(rule => rule.Product == productScope
                    && ((rule.FromUnitOfMeasure == from.RecId && rule.ToUnitOfMeasure == to.RecId)
                        || (rule.FromUnitOfMeasure == to.RecId && rule.ToUnitOfMeasure == from.RecId))))
                    continue;

                db.Set<UnitOfMeasureConversion>().Add(new UnitOfMeasureConversion
                {
                    Product = productScope,
                    FromUnitOfMeasure = from.RecId,
                    ToUnitOfMeasure = to.RecId,
                    Factor = seed.Factor,
                    Numerator = 1,
                    Denominator = 1,
                    DataAreaId = item.DataAreaId,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                });
            }
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
