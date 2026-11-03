using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal sealed class AccountsPayableScenarioMasterDataService
{
    internal async Task SeedAsync(DbContext db, CancellationToken ct)
    {
        foreach (var seed in AccountsPayableScenarioDemoData.Vendors)
        {
            var party = await ExampleSeedStore.EnsureAsync(db, x => x.PartyNumber == seed.Account,
                DirPartyTableDemoData.CreateVendorParty(seed.Account, seed.Name, seed.Name, "sys"), ct);
            if (party.DataAreaId != "dat") throw new InvalidOperationException("An AP example party belongs to another company.");
            var vendor = await ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.AccountNum == seed.Account,
                VendTableDemoData.Create(seed.Account, party.RecId, seed.Group, seed.Currency, seed.TaxGroup, "Monthly", "ERP-BANK", "sys"), ct);
            if (vendor.Party != party.RecId || vendor.VendGroup != seed.Group || vendor.Currency != seed.Currency || vendor.TaxGroup != seed.TaxGroup)
                throw new InvalidOperationException($"AP example vendor {seed.Account} conflicts with existing data; it was preserved.");
        }
        await InventModelGroupSeeder.SeedPurchaseAccrualAsync(db, ct);
        var storage = await db.Set<EcoResStorageDimensionGroup>().IgnoreQueryFilters().SingleAsync(x => x.Name == "SW", ct);
        var tracking = await db.Set<EcoResTrackingDimensionGroup>().IgnoreQueryFilters().SingleAsync(x => x.Name == "NONE", ct);
        foreach (var code in new[] { AccountsPayableScenarioDemoData.Item, AccountsPayableScenarioDemoData.SecondItem })
        {
            var product = await EcoResProductSeeder.SeedAsync(db, code, "Example AP stocked office equipment " + code, ct);
            await EcoResDistinctProductSeeder.SeedAsync(db, product.RecId, ct);
            await EcoResProductTranslationSeeder.SeedAsync(db, product.RecId, product.SearchName, "Fictional basic purchasing example", ct);
            var item = await InventTableSeeder.SeedAsync(db, code, "PCS", ct);
            if (item.Product != 0 && item.Product != product.RecId)
                throw new InvalidOperationException("An AP example released product was customized; it was preserved.");
            if (item.Product == 0) item.Product = product.RecId;
            if (string.IsNullOrWhiteSpace(item.Name)) item.Name = product.SearchName;
            await db.SaveChangesAsync(ct);
            await InventItemGroupItemSeeder.SeedAsync(db, code, "FIN", ct);
            foreach (var module in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
                await InventTableModuleSeeder.SeedAsync(db, code, module, "PCS", 100m, ct);
            await ExampleSeedStore.EnsureAsync(db, x => x.ItemDataAreaId == "dat" && x.ItemId == code,
                new InventModelGroupItem { DataAreaId = "dat", ItemDataAreaId = "dat", ItemId = code,
                    ModelGroupDataAreaId = "dat", ModelGroupId = AccountsPayableScenarioDemoData.ModelGroup }, ct);
            await EcoResStorageDimensionGroupItemSeeder.SeedAsync(db, storage.RecId, code, ct);
            await EcoResTrackingDimensionGroupItemSeeder.SeedAsync(db, tracking.RecId, code, ct);
        }
    }
}
