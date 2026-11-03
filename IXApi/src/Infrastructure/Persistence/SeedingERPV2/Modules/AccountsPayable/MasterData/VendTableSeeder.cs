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
public sealed class VendTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.VendTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.VendParametersSeeder", "ERPV2.VendGroupSeeder", "ERPV2.VendPaymModeTableSeeder", "ERPV2.CashDiscSeeder", "ERPV2.LogisticsElectronicAddressSeeder", "ERPV2.LogisticsPostalAddressSeeder", "ERPV2.InventLocationSeeder", "ERPV2.CurrencySeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var number = $"APV2-{c.Company}-{suffix}";
            var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == number, ct);
            await c.EnsureAsync<VendTable>(x => x.AccountNum == number, new() { AccountNum = number, Party = party.RecId, VendGroup = "APV2", InvoiceAccount = number,
                PaymTermId = "APV2-N30", PaymMode = "APV2", CashDisc = "APV2-2", BankAccount = "APV2", Currency = suffix == "USD" ? "USD" : "SAR", TaxGroup = "APV2",
                InventSiteId = "APV2-SITE", InventLocation = "APV2-WH", MatchingPolicy = MatchingPolicy.ThreeWay }, ct);
        }
    }
}
