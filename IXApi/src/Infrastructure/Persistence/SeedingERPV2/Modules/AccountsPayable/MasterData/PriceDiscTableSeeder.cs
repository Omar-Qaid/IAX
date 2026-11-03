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
public sealed class PriceDiscTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.PriceDiscTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.VendTableSeeder", "ERPV2.InventTableModuleSeeder", "ERPV2.InventDimSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var vendor = $"APV2-{c.Company}-LOCAL";
        var from = new DateTime(c.FiscalYear, 1, 1);
        await c.EnsureAsync<PriceDiscTable>(x => x.AccountRelation == vendor && x.ItemRelation == "APV2-STOCK" && x.Module == ModuleInventCustVend.Vend && x.Relation == PriceType.PricePurch && x.FromDate == from,
            new() { ItemCode = PriceDiscProductCodeType.Table, ItemRelation = "APV2-STOCK", AccountCode = PriceDiscPartyCodeType.Table, AccountRelation = vendor,
                Relation = PriceType.PricePurch, Module = ModuleInventCustVend.Vend, Amount = 100m, PriceUnit = 1m, QuantityAmountFrom = 1m,
                FromDate = from, ToDate = new(c.FiscalYear, 12, 31), Currency = "SAR", UnitId = "EA", InventDimId = "APV2-DIM" }, ct);
    }
}
