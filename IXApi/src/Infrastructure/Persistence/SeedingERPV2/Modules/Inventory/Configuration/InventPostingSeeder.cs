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
public sealed class InventPostingSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventPostingSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder", "ERPV2.InventoryAccountStructureSeeder", "ERPV2.ItemGroupSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var itemGroup = InventoryCycleSeedExample.ItemGroup;
        foreach (var (type, code) in InventoryCycleSeedExample.PostingProfiles)
        {
            var dimension = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == code, ct);
            await c.EnsureAsync<InventPosting>(x => x.InventAccountType == type && x.ItemCode == TableGroupAll.Group && x.ItemRelation == itemGroup && x.CustVendCode == TableGroupAll.All,
                new() { InventAccountType = type, ItemCode = TableGroupAll.Group, ItemRelation = itemGroup, CustVendCode = TableGroupAll.All, LedgerDimension = dimension.RecId }, ct);
        }
    }
}
