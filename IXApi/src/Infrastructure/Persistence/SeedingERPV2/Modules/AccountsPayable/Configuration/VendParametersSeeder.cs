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
public sealed class VendParametersSeeder : IDataSeeder
{
    public string Id => "ERPV2.VendParametersSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.VendPostingSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        await c.EnsureAsync<VendParameters>(x => x.Key == 0, new() { Key = 0, VendPostingProfile = "APV2" }, ct);
    }
}
