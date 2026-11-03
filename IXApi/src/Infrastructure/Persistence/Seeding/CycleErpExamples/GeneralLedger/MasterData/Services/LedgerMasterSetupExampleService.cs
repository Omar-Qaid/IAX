using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal sealed class LedgerMasterSetupExampleService
{
    internal async Task SeedAsync(DbContext db, CancellationToken ct)
    {
        var ledger = await db.Set<Ledger>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && !x.IsDeleted, ct);
        var company = await db.Set<CompanyInfo>().IgnoreQueryFilters().SingleAsync(x => x.DataArea == "dat" && !x.IsDeleted, ct);
        var dimensions = new Dictionary<string, long>();
        foreach (var (code, name, type) in new[] {
            ("60990001", "ERP example cash discount expense", MainAccountType.Expense),
            ("40990001", "ERP example realized exchange gain", MainAccountType.Revenue),
            ("60990002", "ERP example realized exchange loss", MainAccountType.Expense) })
        {
            var account = await MainAccountSeeder.SeedAsync(db, code, name, type, ledger.ChartOfAccounts, ct);
            if (account.Type != type || account.LedgerChartOfAccounts != ledger.ChartOfAccounts)
                throw new InvalidOperationException($"The example account {code} conflicts with existing configuration; it was preserved.");
            dimensions[code] = (await SeedLedgerDimensionResolver.ResolveAsync(db, account, ct)).RecId;
            await MainAccountLegalEntitySeeder.SeedAsync(db, account.RecId, company.RecId, ct);
        }
        await CashDiscSeeder.SeedAsync(db, dimensions["60990001"], ct);
        _ = await db.Set<Currency>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == "dat" && x.CurrencyCode == "USD" && !x.IsDeleted, ct);
        foreach (var (type, code) in new[] {
            (CurrencyGainLossAccountType.RealizedGain, "40990001"), (CurrencyGainLossAccountType.RealizedLoss, "60990002") })
        {
            await LedgerGainLossAccountSeeder.SeedAsync(db, ledger.RecId, type, dimensions[code], ct);
            await CurrencyLedgerGainLossAccountSeeder.SeedAsync(db, ledger.RecId, type, dimensions[code], ct);
        }
        var periods = await db.Set<LedgerFiscalCalendarPeriod>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == "dat" && x.Ledger == ledger.RecId && !x.IsDeleted).Select(x => x.RecId).ToListAsync(ct);
        foreach (var period in periods)
            await LedgerPeriodModuleAccessControlSeeder.SeedAsync(db, period, ct);
    }
}
