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
        foreach (var (code, name, type) in new[] {
            ("181001", "AP example bank", MainAccountType.Asset), ("181002", "AP example financial inventory", MainAccountType.Asset),
            ("181003", "AP example receipt asset", MainAccountType.Asset), ("181004", "AP example input tax", MainAccountType.Asset),
            ("181005", "AP example advances", MainAccountType.Asset), ("281001", "AP example vendor control", MainAccountType.Liability),
            ("281002", "AP example receipt accrual", MainAccountType.Liability), ("481001", "AP example cash discount", MainAccountType.Revenue),
            ("481002", "AP example FX gain", MainAccountType.Revenue), ("681001", "AP example expense", MainAccountType.Expense), ("681002", "AP example FX loss", MainAccountType.Expense) })
            await c.EnsureAsync<MainAccount>(x => x.MainAccountId == code && x.LedgerChartOfAccounts == chart.RecId,
                new() { MainAccountId = code, Name = name, Type = type, LedgerChartOfAccounts = chart.RecId }, ct);
    }
}
