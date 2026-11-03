using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public sealed class LedgerExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.GeneralLedger.Examples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] {
        "Finance.GeneralLedger.MainAccounts", "Finance.GeneralLedger.Currencies", "Finance.GeneralLedger.FiscalCalendars",
        "Finance.AccountsReceivable.CustLedger", "Finance.AccountsReceivable.DemoCustVend" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        var company = await db.Set<CompanyInfo>().IgnoreQueryFilters().SingleAsync(x => x.DataArea == "dat", ct);
        var calendar = await db.Set<FiscalCalendar>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.CalendarId == "HBMC", ct);
        var rates = await db.Set<ExchangeRateType>().IgnoreQueryFilters().Where(x => x.DataAreaId == "dat")
            .ToDictionaryAsync(x => x.Name, x => x.RecId, ct);
        var ledger = await db.Set<Ledger>().IgnoreQueryFilters().SingleOrDefaultAsync(x => x.DataAreaId == "dat", ct);
        if (ledger == null)
        {
            var chart = await db.Set<LedgerChartOfAccounts>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.Name == "DEMO-COA", ct);
            if (chart == null)
            {
                chart = new LedgerChartOfAccounts { Name = "DEMO-COA", Description = "Example trading company chart of accounts",
                    MainAccountFormatMask = "########", DataAreaId = "dat", CreatedBy = "sys", OwnerAccountId = "sys" };
                db.Add(chart);
                await db.SaveChangesAsync(ct);
            }
            ledger = new Ledger { Name = "DAT", Description = "Example trading company ledger", DataAreaId = "dat",
                ChartOfAccounts = chart.RecId, FiscalCalendar = calendar.RecId, PrimaryForLegalEntity = company.RecId,
                AccountingCurrency = "SAR", ReportingCurrency = "SAR", DefaultExchangeRateType = rates["Default"],
                ReportingCurrencyExchangeRateType = rates["Default"], BudgetExchangeRateType = rates["Budget"],
                CreatedBy = "sys", OwnerAccountId = "sys" };
            db.Add(ledger);
            await db.SaveChangesAsync(ct);
        }
        // Associate only known example accounts; never claim arbitrary user-created accounts.
        var exampleAccountCodes = new[] { "10010000", "10020000", "11010000", "14010000", "11020005", "11020010",
            "11020020", "11020099", "11030000", "11040000", "11050000", "11090001", "20010000", "22010005",
            "30010000", "40010000", "40910000", "50010000", "50910000", "51030001", "51040001" };
        var accounts = await db.Set<MainAccount>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == "dat" && x.LedgerChartOfAccounts == 0 && exampleAccountCodes.Contains(x.MainAccountId)).ToListAsync(ct);
        foreach (var account in accounts) account.LedgerChartOfAccounts = ledger.ChartOfAccounts;
        await db.SaveChangesAsync(ct);
        // The calendar seeder fills missing ledger/year/period links without replacing existing ones.
        await new FiscalCalendarSeeder().SeedAsync(context, ct);
        var seedAccounts = await db.Set<MainAccount>().IgnoreQueryFilters().Where(x => x.DataAreaId == "dat")
            .ToDictionaryAsync(x => x.MainAccountId, ct);
        var lines = await db.Set<CustLedgerAccounts>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == "dat" && x.SummaryLedgerDimension == 0
                && (x.PostingProfile == "GEN" || x.PostingProfile == "PrePayment" || x.PostingProfile == "EXPORT" || x.PostingProfile == "GOV"))
            .ToListAsync(ct);
        foreach (var line in lines)
        {
            // Limit repairs to known example keys, leaving custom posting lines alone.
            if (line.AccountCode == AccountCode.Table && line.Num != "C00015" && line.Num != "CUST-300") continue;
            if (line.AccountCode == AccountCode.Group && !new[] { "Consultant", "Contractor", "Government", "IKKAff", "SisComp", "Traders", "CUST-RTL", "INT" }.Contains(line.Num)) continue;
            var code = line.PostingProfile switch {
                "PrePayment" => "11020010", "EXPORT" => "11020020", "GOV" => "11030000",
                _ => line.Num switch { "Government" => "11030000", "Traders" => "11040000",
                    "C00015" => "11050000", "CUST-300" => "11020020", _ => "11010000" }
            };
            var account = seedAccounts[code];
            var dimension = await db.Set<DimensionAttributeValueCombination>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DataAreaId == "dat" && x.MainAccount == account.RecId && x.DisplayValue == code, ct);
            if (dimension == null)
            {
                dimension = new DimensionAttributeValueCombination { DataAreaId = "dat", MainAccount = account.RecId,
                    MainAccountValue = code, DisplayValue = code, CreatedBy = "sys", OwnerAccountId = "sys" };
                db.Add(dimension);
                await db.SaveChangesAsync(ct);
            }
            line.SummaryLedgerDimension = dimension.RecId;
        }
        await db.SaveChangesAsync(ct);
    }
}

