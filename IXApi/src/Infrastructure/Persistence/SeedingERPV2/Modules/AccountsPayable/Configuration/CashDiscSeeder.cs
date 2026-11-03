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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
public sealed class CashDiscSeeder : IDataSeeder
{
    public string Id => "ERPV2.CashDiscSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var dimension = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == "481001", ct);
        await c.EnsureAsync<CashDisc>(x => x.CashDiscCode == "APV2-2", new() { CashDiscCode = "APV2-2", Description = "AP example 2 percent in 10 days", NumOfDays = 10, DiscPct = 2m, CashDiscLedgerDimension = dimension.RecId }, ct);
    }
}
