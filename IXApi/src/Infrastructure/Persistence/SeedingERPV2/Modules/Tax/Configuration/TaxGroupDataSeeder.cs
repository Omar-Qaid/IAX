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
public sealed class TaxGroupDataSeeder : IDataSeeder
{
    public string Id => "ERPV2.TaxGroupDataSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.TaxTableSeeder", "ERPV2.TaxGroupHeadingSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        await c.EnsureAsync<TaxGroupData>(x => x.TaxGroup == "APV2" && x.TaxCode == "APV2-15", new() { TaxGroup = "APV2", TaxCode = "APV2-15", TaxExemptCode = "APV2-NONE" }, ct);
    }
}
