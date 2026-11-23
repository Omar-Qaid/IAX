using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

/// <summary>
/// The company ledger on the baseline calendar, with every calendar year and period open on it. Subledger posting
/// needs exactly one ledger with an accounting currency and an open period, so an existing ledger only has its
/// empty currency, calendar or legal entity filled in; nothing it already holds is changed.
/// </summary>
public sealed class BaselineLedgerSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.GeneralLedger.Ledger";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies =>
        [BaselineCompanySeeder.SeederId, BaselineFiscalCalendarSeeder.SeederId, BaselineCurrencySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var definition = GeneralLedgerBaselineCatalog.Load().Ledger;
        var company = await c.Rows<CompanyInfo>().SingleAsync(x => x.DataArea == c.Company, ct);
        var calendar = await c.Rows<FiscalCalendar>().SingleAsync(x => x.CalendarId == definition.CalendarId, ct);
        var rateType = await c.Rows<ExchangeRateType>().FirstOrDefaultAsync(x => x.Name == definition.DefaultExchangeRateType, ct);

        var ledger = await c.AddMissingAsync<Ledger>(x => true, new()
        {
            Name = definition.CalendarId, Description = definition.Description, PrimaryForLegalEntity = company.RecId,
            FiscalCalendar = calendar.RecId, AccountingCurrency = definition.AccountingCurrency, DefaultExchangeRateType = rateType?.RecId ?? 0
        }, ct);
        if (string.IsNullOrWhiteSpace(ledger.AccountingCurrency)) ledger.AccountingCurrency = definition.AccountingCurrency;
        if (ledger.FiscalCalendar == 0) ledger.FiscalCalendar = calendar.RecId;
        if (ledger.PrimaryForLegalEntity == 0) ledger.PrimaryForLegalEntity = company.RecId;
        await c.Db.SaveChangesAsync(ct);

        foreach (var year in await c.Db.Set<FiscalCalendarYear>().IgnoreQueryFilters().Where(x => x.FiscalCalendar == calendar.RecId).ToListAsync(ct))
            await c.AddMissingAsync<LedgerFiscalCalendarYear>(x => x.Ledger == ledger.RecId && x.FiscalCalendarYear == year.RecId,
                new() { Ledger = ledger.RecId, FiscalCalendarYear = year.RecId, Status = FiscalPeriodStatus.Open }, ct);
        foreach (var period in await c.Db.Set<FiscalCalendarPeriod>().IgnoreQueryFilters().Where(x => x.FiscalCalendar == calendar.RecId).ToListAsync(ct))
            await c.AddMissingAsync<LedgerFiscalCalendarPeriod>(x => x.Ledger == ledger.RecId && x.FiscalCalendarPeriod == period.RecId,
                new() { Ledger = ledger.RecId, FiscalCalendarPeriod = period.RecId, Status = FiscalPeriodStatus.Open }, ct);
    }
}
