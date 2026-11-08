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
public sealed class InventModelGroupSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventModelGroupSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        await c.EnsureAsync<InventModelGroup>(x => x.ModelGroupId == "APV2", new() { ModelGroupId = "APV2", Name = "AP example weighted average stocked", InventModel = InventModel.WeightedAvg, StockedProduct = NoYes.Yes, PostOnHandPhysical = NoYes.Yes, PostOnHandFinancial = NoYes.Yes, PostPhysicalPurchase = NoYes.Yes }, ct);
        await c.EnsureAsync<InventModelGroup>(x => x.ModelGroupId == "APV2-SVC",
            new() { ModelGroupId = "APV2-SVC", Name = "AP example nonstocked service", InventModel = InventModel.WeightedAvg, StockedProduct = NoYes.No }, ct);
    }
}
