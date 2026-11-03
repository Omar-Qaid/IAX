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
public sealed class InventTableModuleSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventTableModuleSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.InventTableSeeder", "ERPV2.UnitOfMeasureSeeder", "ERPV2.TaxOnItemSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var item in new[] { "APV2-STOCK", "APV2-SVC" })
            foreach (var module in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
                await c.EnsureAsync<InventTableModule>(x => x.ItemId == item && x.ModuleType == module,
                    new() { ItemId = item, ModuleType = module, UnitId = "EA", Price = 100m, PriceQty = 1m, PriceUnit = 1m, TaxItemGroupId = "APV2" }, ct);
    }
}
