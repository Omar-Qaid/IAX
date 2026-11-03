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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
public sealed class ExchangeRateCurrencyPairSeeder : IDataSeeder
{
    public string Id => "ERPV2.ExchangeRateCurrencyPairSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.ExchangeRateTypeSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var type = await c.RequireAsync<ExchangeRateType>(x => x.Name == "APV2", ct);
        await c.EnsureAsync<ExchangeRateCurrencyPair>(x => x.ExchangeRateType == type.RecId && x.FromCurrencyCode == "USD" && x.ToCurrencyCode == "SAR",
            new() { ExchangeRateType = type.RecId, FromCurrencyCode = "USD", ToCurrencyCode = "SAR", ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One }, ct);
    }
}
