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
            ("181001", "Treasury bank (demo)", MainAccountType.Asset), ("181002", "Office paper inventory (demo)", MainAccountType.Asset),
            ("181003", "Received office supplies - interim (demo)", MainAccountType.Asset), ("181004", "Recoverable purchase tax (demo)", MainAccountType.Asset),
            ("181005", "Supplier advances (demo)", MainAccountType.Asset), ("281001", "Trade accounts payable (demo)", MainAccountType.Liability),
            ("281002", "Office supplies received not invoiced (demo)", MainAccountType.Liability), ("481001", "Purchase cash discounts (demo)", MainAccountType.Revenue),
            ("481002", "Realized exchange gains (demo)", MainAccountType.Revenue), ("681001", "Printer maintenance expense (demo)", MainAccountType.Expense), ("681002", "Realized exchange losses (demo)", MainAccountType.Expense), ("681003", "Supplier delivery expense (demo)", MainAccountType.Expense) })
            await c.EnsureAsync<MainAccount>(x => x.MainAccountId == code && x.LedgerChartOfAccounts == chart.RecId,
                new() { MainAccountId = code, Name = name, Type = type, LedgerChartOfAccounts = chart.RecId }, ct);
    }
}
