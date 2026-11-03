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
public sealed class InvoiceMatchingPolicySeeder : IDataSeeder
{
    public string Id => "ERPV2.InvoiceMatchingPolicySeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.VendTableSeeder", "ERPV2.InventTableSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        // Matching is stored on vendors and items in the current repository.
        foreach (var vendor in await c.Rows<VendTable>().Where(x => x.VendGroup == "APV2").ToListAsync(ct))
            if (vendor.MatchingPolicy != MatchingPolicy.ThreeWay) throw new InvalidOperationException("AP vendor matching policy differs.");
        var item = await c.RequireAsync<InventTable>(x => x.ItemId == "APV2-STOCK", ct);
        if (item.MatchingPolicy != MatchingPolicy.ThreeWay) throw new InvalidOperationException("AP item matching policy differs.");
    }
}
