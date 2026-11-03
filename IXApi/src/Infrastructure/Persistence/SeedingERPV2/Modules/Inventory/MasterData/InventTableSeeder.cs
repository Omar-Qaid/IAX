using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class InventTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.EcoResProductSeeder", "ERPV2.InventModelGroupSeeder", "ERPV2.ItemGroupSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var suffix in new[] { "STOCK", "SVC" })
        {
            var number = $"APV2-{c.Company}-{suffix}";
            var product = await c.RequireAsync<EcoResProduct>(x => x.DisplayProductNumber == number, ct);
            var item = "APV2-" + suffix;
            await c.EnsureAsync<InventTable>(x => x.ItemId == item, new() { ItemId = item, Product = product.RecId, Name = "AP example " + suffix, NameAlias = "AP example " + suffix, ItemType = suffix == "STOCK" ? ItemType.Item : ItemType.Service, MatchingPolicy = suffix == "STOCK" ? MatchingPolicy.ThreeWay : MatchingPolicy.TwoWay }, ct);
            await c.EnsureAsync<InventItemGroupItem>(x => x.ItemId == item && x.ItemDataAreaId == c.Company,
                new() { ItemId = item, ItemGroupId = "APV2", ItemDataAreaId = c.Company, ItemGroupDataAreaId = c.Company }, ct);
            await c.EnsureAsync<InventModelGroupItem>(x => x.ItemId == item && x.ItemDataAreaId == c.Company,
                new() { ItemId = item, ModelGroupId = suffix == "STOCK" ? "APV2" : "APV2-SVC", ItemDataAreaId = c.Company, ModelGroupDataAreaId = c.Company }, ct);
        }
    }
}
