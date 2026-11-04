using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
public sealed class MarkupTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.MarkupTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder", "ERPV2.TaxItemGroupHeadingSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var expense = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == "681003", ct);
        await c.RequireAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == "APV2", ct);
        await c.EnsureAsync<MarkupTable>(x => x.MarkupCode == "APV2-FRT" && x.ModuleType == ModuleInventPurchSales.Purchase,
            new() { MarkupCode = "APV2-FRT", Txt = "Demo supplier delivery charge", ModuleType = ModuleInventPurchSales.Purchase,
                CustType = 0, CustomerLedgerDimension = expense.RecId, VendType = 1,
                TaxItemGroup = "APV2", UseInMatching = NoYes.Yes }, ct);
    }
}
