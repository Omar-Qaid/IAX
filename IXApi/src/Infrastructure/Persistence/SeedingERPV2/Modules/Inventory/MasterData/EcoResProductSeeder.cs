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
public sealed class EcoResProductSeeder : IDataSeeder
{
    public string Id => "ERPV2.EcoResProductSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var suffix in new[] { "STOCK", "SVC", "BATCH", "SERIAL" })
        {
            var number = $"APV2-{c.Company}-{suffix}";
            await c.EnsureAsync<EcoResProduct>(x => x.DisplayProductNumber == number,
                new() { DisplayProductNumber = number, SearchName = suffix switch { "STOCK" => "A4-PAPER-REAM", "BATCH" => "TONER-BATCH", "SERIAL" => "LABELPRN-SERIAL", _ => "PRINTER-MAINT" }, ProductType = suffix == "SVC" ? ItemType.Service : ItemType.Item }, ct);
        }
    }
}
