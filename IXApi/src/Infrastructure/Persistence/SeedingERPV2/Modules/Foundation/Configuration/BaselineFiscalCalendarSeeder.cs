using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>
/// The company calendar: one year with an opening period (P0), twelve monthly operating periods and a closing
/// period (P13). A period is matched by name or by type and dates, so a renamed period is not added twice.
/// Opening the year and its periods on the ledger belongs to <c>BaselineLedgerSeeder</c>.
/// </summary>
public sealed class BaselineFiscalCalendarSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.GeneralLedger.FiscalCalendars";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var definition = FoundationBaselineCatalog.Load().FiscalCalendar;
        var calendar = await c.AddMissingAsync<FiscalCalendar>(x => x.CalendarId == definition.CalendarId,
            new() { CalendarId = definition.CalendarId, Description = definition.Description }, ct);
        var start = Utc(definition.Year, 1, 1);
        var end = Utc(definition.Year, 12, 31);
        var year = await c.AddMissingAsync<FiscalCalendarYear>(x => x.FiscalCalendar == calendar.RecId && x.Name == definition.Year.ToString(),
            new() { FiscalCalendar = calendar.RecId, Name = definition.Year.ToString(), StartDate = start, EndDate = end }, ct);

        var existing = await c.Rows<FiscalCalendarPeriod>()
            .Where(x => x.FiscalCalendar == calendar.RecId && x.FiscalCalendarYear == year.RecId).ToListAsync(ct);
        foreach (var period in Periods(definition.Year))
        {
            if (existing.Any(x => x.Name == period.Name || (x.Type == period.Type && x.StartDate == period.StartDate && x.EndDate == period.EndDate)))
                continue;
            period.FiscalCalendar = calendar.RecId;
            period.FiscalCalendarYear = year.RecId;
            await c.AddMissingAsync<FiscalCalendarPeriod>(x => x.FiscalCalendarYear == year.RecId && x.Name == period.Name, period, ct);
        }
    }

    private static IEnumerable<FiscalCalendarPeriod> Periods(int year)
    {
        yield return Period(0, FiscalPeriodType.Opening, Utc(year, 1, 1), Utc(year, 1, 1), 1);
        for (var month = 1; month <= 12; month++)
        {
            var first = Utc(year, month, 1);
            yield return Period(month, FiscalPeriodType.Operating, first, first.AddMonths(1).AddDays(-1), month);
        }
        yield return Period(13, FiscalPeriodType.Closing, Utc(year, 12, 31), Utc(year, 12, 31), 12);
    }

    private static FiscalCalendarPeriod Period(int number, FiscalPeriodType type, DateTime from, DateTime to, int month) => new()
    {
        Name = "Period " + number, ShortName = "P" + number, Type = type, StartDate = from, EndDate = to,
        Month = month, Quarter = (month - 1) / 3 + 1
    };

    private static DateTime Utc(int year, int month, int day) => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
}
