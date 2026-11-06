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
public sealed class CustTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.CustTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CustParametersSeeder", "ERPV2.CustGroupSeeder", "ERPV2.CustPaymModeTableSeeder", "ERPV2.SalesCashDiscSeeder", "ERPV2.CustElectronicAddressSeeder", "ERPV2.CustPostalAddressSeeder", "ERPV2.InventLocationSeeder", "ERPV2.CurrencySeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var number = $"ARV2-{c.Company}-{suffix}";
            var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == number, ct);
            await c.EnsureAsync<CustTable>(x => x.AccountNum == number, new() { AccountNum = number, Party = party.RecId, CustGroupId = "ARV2",
                InvoiceAccount = number, PaymTermId = "APV2-N30", PaymModeId = "ARV2",
                CurrencyCode = suffix == "USD" ? "USD" : "SAR", TaxGroupId = "ARV2", InventSiteId = "APV2-SITE", InventLocationId = "APV2-WH",
                Blocked = CustVendorBlocked.No }, ct);
        }
    }
}
