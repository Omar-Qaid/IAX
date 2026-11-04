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
public sealed class MainAccountSeeder : IDataSeeder
{
    public string Id => "ERPV2.MainAccountSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.ChartOfAccountsSeeder", "ERPV2.CurrencySeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var chart = await c.RequireAsync<LedgerChartOfAccounts>(x => x.Name == "APV2", ct);
        foreach (var (code, name, type) in GeneralLedgerSeedExample.MainAccounts)
            await c.EnsureAsync<MainAccount>(x => x.MainAccountId == code && x.LedgerChartOfAccounts == chart.RecId,
                new() { MainAccountId = code, Name = name, Type = type, LedgerChartOfAccounts = chart.RecId }, ct);
    }
}
