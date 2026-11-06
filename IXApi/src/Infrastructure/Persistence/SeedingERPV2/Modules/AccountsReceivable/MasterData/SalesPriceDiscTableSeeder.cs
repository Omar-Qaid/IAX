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
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>Sales trade agreements for the shared demo items, in each customer's own currency.</summary>
public sealed class SalesPriceDiscTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.SalesPriceDiscTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CustTableSeeder", "ERPV2.InventTableModuleSeeder", "ERPV2.InventDimSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var from = new DateTime(c.FiscalYear, 1, 1);
        foreach (var (suffix, item, currency, amount) in new[] {
            ("LOCAL", InventorySeedExample.StockItem, "SAR", CustomerSeedExample.PaperPriceSar),
            ("LOCAL", InventorySeedExample.ServiceItem, "SAR", CustomerSeedExample.MaintenancePriceSar),
            ("USD", InventorySeedExample.StockItem, "USD", CustomerSeedExample.PaperPriceUsd) })
        {
            var customer = $"ARV2-{c.Company}-{suffix}";
            await c.EnsureAsync<PriceDiscTable>(x => x.AccountRelation == customer && x.ItemRelation == item && x.Module == ModuleInventCustVend.Cust
                && x.Relation == PriceType.PriceSales && x.FromDate == from,
                new() { ItemCode = PriceDiscProductCodeType.Table, ItemRelation = item, AccountCode = PriceDiscPartyCodeType.Table, AccountRelation = customer,
                    Relation = PriceType.PriceSales, Module = ModuleInventCustVend.Cust, Amount = amount, PriceUnit = 1m, QuantityAmountFrom = 1m,
                    FromDate = from, ToDate = new(c.FiscalYear, 12, 31), Currency = currency, UnitId = "EA", InventDimId = "APV2-DIM" }, ct);
        }
    }
}
