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
public sealed class VendPostingSeeder : IDataSeeder
{
    public string Id => "ERPV2.VendPostingSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.VendGroupSeeder", "ERPV2.DimensionAttributeValueCombinationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var summary = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == "281001", ct);
        var advance = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == "181005", ct);
        // The supplied VendPosting concept is represented by VendLedgerAccounts in this repository.
        await c.EnsureAsync<VendLedgerAccounts>(x => x.PostingProfile == "APV2" && x.AccountCode == AccountCode.Group && x.Num == "APV2",
            new() { PostingProfile = "APV2", AccountCode = AccountCode.Group, Num = "APV2", SummaryLedgerDimension = summary.RecId, ClearingLedgerDimension = advance.RecId }, ct);
    }
}
