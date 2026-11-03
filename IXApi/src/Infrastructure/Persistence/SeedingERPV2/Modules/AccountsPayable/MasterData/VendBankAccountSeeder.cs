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
public sealed class VendBankAccountSeeder : IDataSeeder
{
    public string Id => "ERPV2.VendBankAccountSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.VendTableSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var vendor = $"APV2-{c.Company}-{suffix}";
            await c.EnsureAsync<VendBankAccount>(x => x.VendAccount == vendor && x.AccountID == "APV2", new() { AccountID = "APV2", VendAccount = vendor, AccountNum = "DEMO-ONLY", Name = "Synthetic vendor bank", CurrencyCode = suffix == "USD" ? "USD" : "SAR" }, ct);
        }
    }
}
