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
public sealed class VendGroupSeeder : IDataSeeder
{
    public string Id => "ERPV2.VendGroupSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.PaymTermSeeder", "ERPV2.TaxGroupDataSeeder", "ERPV2.ExchangeRateTypeSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var rate = await c.RequireAsync<ExchangeRateType>(x => x.Name == "APV2", ct);
        await c.EnsureAsync<VendGroup>(x => x.VendGroupCode == "APV2", new() { VendGroupCode = "APV2", Name = "AP example vendors", PaymTermId = "APV2-N30", TaxGroupId = "APV2", AccountingCurrencyExchangeRateType = rate.RecId }, ct);
    }
}
