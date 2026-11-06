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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
public sealed class CustParametersSeeder : IDataSeeder
{
    public string Id => "ERPV2.CustParametersSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CustPostingSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.EnsureAsync<CustParameters>(x => x.Key == 0, new() { Key = 0, CustPostingProfile = "ARV2" }, ct);
    }
}
