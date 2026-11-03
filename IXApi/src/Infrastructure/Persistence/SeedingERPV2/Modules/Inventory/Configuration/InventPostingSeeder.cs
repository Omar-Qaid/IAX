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
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder", "ERPV2.ItemGroupSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var (type, code) in new[] { (InventAccountType.PurchPackingSlip, "181003"), (InventAccountType.PurchPackingSlipOffsetAccount, "281002"), (InventAccountType.PurchReceipt, "181002"), (InventAccountType.PurchConsump, "681001"), (InventAccountType.PurchDisc, "481001") })
        {
            var dimension = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == code, ct);
            await c.EnsureAsync<InventPosting>(x => x.InventAccountType == type && x.ItemCode == TableGroupAll.Group && x.ItemRelation == "APV2" && x.CustVendCode == TableGroupAll.All,
                new() { InventAccountType = type, ItemCode = TableGroupAll.Group, ItemRelation = "APV2", CustVendCode = TableGroupAll.All, LedgerDimension = dimension.RecId }, ct);
        }
    }
}
