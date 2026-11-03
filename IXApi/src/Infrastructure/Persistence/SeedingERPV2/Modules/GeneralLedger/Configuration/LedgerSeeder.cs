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
public sealed class LedgerSeeder : IDataSeeder
{
    public string Id => "ERPV2.LedgerSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.ChartOfAccountsSeeder", "ERPV2.FiscalCalendarSeeder", "ERPV2.ExchangeRateTypeSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var company = await c.Rows<CompanyInfo>().Where(x => !x.IsDeleted && x.DataArea == c.Company).SingleOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Create the legal entity before phase-one seeding.");
        var chart = await c.RequireAsync<LedgerChartOfAccounts>(x => x.Name == "APV2", ct);
        var calendar = await c.RequireAsync<FiscalCalendar>(x => x.CalendarId == "APV2", ct);
        var rate = await c.RequireAsync<ExchangeRateType>(x => x.Name == "APV2", ct);
        var ledger = await c.EnsureAsync<Ledger>(x => x.PrimaryForLegalEntity == company.RecId,
            new() { Name = "APV2", Description = "AP example ledger", PrimaryForLegalEntity = company.RecId, ChartOfAccounts = chart.RecId,
                FiscalCalendar = calendar.RecId, AccountingCurrency = "SAR", DefaultExchangeRateType = rate.RecId }, ct);
        var yearName = c.FiscalYear.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var year = await c.RequireAsync<FiscalCalendarYear>(x => x.FiscalCalendar == calendar.RecId && x.Name == yearName, ct);
        await c.EnsureAsync<LedgerFiscalCalendarYear>(x => x.Ledger == ledger.RecId && x.FiscalCalendarYear == year.RecId,
            new() { Ledger = ledger.RecId, FiscalCalendarYear = year.RecId, Status = FiscalPeriodStatus.Open }, ct);
        foreach (var period in await c.Rows<FiscalCalendarPeriod>().Where(x => x.FiscalCalendarYear == year.RecId).ToListAsync(ct))
            await c.EnsureAsync<LedgerFiscalCalendarPeriod>(x => x.Ledger == ledger.RecId && x.FiscalCalendarPeriod == period.RecId,
                new() { Ledger = ledger.RecId, FiscalCalendarPeriod = period.RecId, Status = FiscalPeriodStatus.Open }, ct);
    }
}
