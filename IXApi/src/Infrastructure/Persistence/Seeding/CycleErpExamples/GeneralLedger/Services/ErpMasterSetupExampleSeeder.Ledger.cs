using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

public sealed partial class ErpMasterSetupExampleSeeder
{
    private async Task SeedLedgerSetupAsync(DbContext db, CancellationToken ct)
    {
        var ledger = await db.Set<Ledger>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == Scope && !x.IsDeleted, ct);
        var company = await db.Set<CompanyInfo>().IgnoreQueryFilters().SingleAsync(x => x.DataArea == Scope && !x.IsDeleted, ct);
        var dimensions = new Dictionary<string, long>();
        foreach (var (code, name, type) in new[] {
            ("60990001", "ERP example cash discount expense", MainAccountType.Expense),
            ("40990001", "ERP example realized exchange gain", MainAccountType.Revenue),
            ("60990002", "ERP example realized exchange loss", MainAccountType.Expense) })
        {
            var account = await EnsureAsync(db, x => x.DataAreaId == Scope && x.MainAccountId == code,
                new MainAccount { DataAreaId = Scope, MainAccountId = code, Name = name,
                    Type = type, LedgerChartOfAccounts = ledger.ChartOfAccounts,
                    CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
            if (account.Type != type || account.LedgerChartOfAccounts != ledger.ChartOfAccounts)
                throw new InvalidOperationException($"The example account {code} conflicts with existing configuration; it was preserved.");
            dimensions[code] = (await SeedLedgerDimensionResolver.ResolveAsync(db, account, ct)).RecId;
            await EnsureAsync(db, x => x.MainAccount == account.RecId && x.LegalEntity == company.RecId,
                new MainAccountLegalEntity { MainAccount = account.RecId, LegalEntity = company.RecId,
                    CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
        }
        await EnsureAsync(db, x => x.DataAreaId == Scope && x.CashDiscCode == "ERP2P10",
            new CashDisc { DataAreaId = Scope, CashDiscCode = "ERP2P10", Description = "ERP example: 2 percent within 10 days",
                NumOfDays = 10, DiscPct = 2m, CashDiscLedgerDimension = dimensions["60990001"],
                CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
        _ = await db.Set<Currency>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == Scope && x.CurrencyCode == "USD" && !x.IsDeleted, ct);
        foreach (var (type, code) in new[] {
            (CurrencyGainLossAccountType.RealizedGain, "40990001"), (CurrencyGainLossAccountType.RealizedLoss, "60990002") })
        {
            await EnsureAsync(db, x => x.DataAreaId == Scope && x.Ledger == ledger.RecId && x.AccountType == type,
                new LedgerGainLossAccount { DataAreaId = Scope, Ledger = ledger.RecId, AccountType = type,
                    LedgerDimension = dimensions[code], CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
            await EnsureAsync(db, x => x.DataAreaId == Scope && x.Ledger == ledger.RecId && x.CurrencyCode == "USD" && x.AccountType == type,
                new CurrencyLedgerGainLossAccount { DataAreaId = Scope, Ledger = ledger.RecId, CurrencyCode = "USD", AccountType = type,
                    LedgerDimension = dimensions[code], CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
        }
        var periods = await db.Set<LedgerFiscalCalendarPeriod>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == Scope && x.Ledger == ledger.RecId && !x.IsDeleted).Select(x => x.RecId).ToListAsync(ct);
        foreach (var period in periods)
            await EnsureAsync(db, x => x.DataAreaId == Scope && x.LedgerFiscalCalendarPeriod == period,
                new LedgerPeriodModuleAccessControl { DataAreaId = Scope, LedgerFiscalCalendarPeriod = period,
                    LedgerAccessLevel = LedgerPeriodAccessLevel.All, TaxAccessLevel = LedgerPeriodAccessLevel.All,
                    BankAccessLevel = LedgerPeriodAccessLevel.All, CustAccessLevel = LedgerPeriodAccessLevel.All,
                    VendAccessLevel = LedgerPeriodAccessLevel.All, InventAccessLevel = LedgerPeriodAccessLevel.All,
                    CreatedBy = "sys", OwnerAccountId = "sys" }, ct);
    }
}
