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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
public sealed class TaxPostingSeeder : IDataSeeder
{
    public string Id => "ERPV2.TaxPostingSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var input = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == "181004", ct);
        await c.EnsureAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == "APV2", new() { TaxAccountGroup = "APV2", Name = "AP example input tax", TaxIncomingLedgerDimension = input.RecId }, ct);
    }
}
