using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal sealed class ProductExampleService
{
    internal async Task SeedDataAsync(DbContext db, CancellationToken cancellationToken = default)
        {
            var createdBy = "sys";

            var groupCodes = new[] { "RAW", "FIN", "CONS", "SPR" };
            var groupIdByCode = await db.Set<InventItemGroup>().IgnoreQueryFilters()
                .Where(g => g.DataAreaId == "dat" && groupCodes.Contains(g.ItemGroupId))
                .ToDictionaryAsync(g => g.ItemGroupId, g => g.RecId, cancellationToken);
            var uomIdByCode = await db.Set<UnitOfMeasure>().IgnoreQueryFilters().ToDictionaryAsync(u => u.Symbol, u => u.RecId, cancellationToken);

            var itemSeeds = EcoResProductDemoData.Items;


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
                    product = await EcoResProductSeeder.SeedAsync(db, seed.Code, seed.Name, cancellationToken);
                }
                await EcoResDistinctProductSeeder.SeedAsync(db, product.RecId, cancellationToken);
                await EcoResProductTranslationSeeder.SeedAsync(db, product.RecId, seed.Name, seed.Description, cancellationToken);
                item ??= await InventTableSeeder.SeedAsync(db, seed.Code, baseRow.Uom, cancellationToken);
                item.Product = product.RecId;
                if (string.IsNullOrWhiteSpace(item.Name)) item.Name = seed.Name;
                await InventItemGroupItemSeeder.SeedAsync(db, seed.Code, seed.Group, cancellationToken);
                await InventTableModuleSeeder.SeedAsync(db, seed.Code, ModuleInventPurchSales.Inventory, baseRow.Uom, 10m, cancellationToken);
                await InventTableModuleSeeder.SeedAsync(db, seed.Code, ModuleInventPurchSales.Purchase, buyRow.Uom, 12m, cancellationToken);
                await InventTableModuleSeeder.SeedAsync(db, seed.Code, ModuleInventPurchSales.Sales, sellRow.Uom, 15m, cancellationToken);
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

                await UnitOfMeasureConversionSeeder.SeedAsync(db, productScope, from.RecId, to.RecId,
                    seed.Factor, item.DataAreaId, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);
        }
}
