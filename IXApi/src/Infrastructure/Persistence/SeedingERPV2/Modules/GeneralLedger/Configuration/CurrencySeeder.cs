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
public sealed class CurrencySeeder : IDataSeeder
{
    public string Id => "ERPV2.CurrencySeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var (code, label) in new[] { ("SAR", "Saudi Riyal"), ("USD", "US Dollar") })
            await c.EnsureAsync<Currency>(x => x.CurrencyCode == code, new() { CurrencyCode = code, CurrencyCodeIso = code, Txt = label, Symbol = code,
                RoundingPrecision = 0.01m, RoundOffPurch = 0.01m, RoundOffSales = 0.01m }, ct);
    }
}
