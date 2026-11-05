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
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public sealed class FiscalCalendarSeeder : IDataSeeder
{
    public string Id => "ERPV2.FiscalCalendarSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var cal = await c.EnsureAsync<FiscalCalendar>(x => x.CalendarId == "APV2", new() { CalendarId = "APV2", Description = "AP example calendar" }, ct);
        var name = c.FiscalYear.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var year = await c.EnsureAsync<FiscalCalendarYear>(x => x.FiscalCalendar == cal.RecId && x.Name == name,
            new() { FiscalCalendar = cal.RecId, Name = name, StartDate = new(c.FiscalYear, 1, 1), EndDate = new(c.FiscalYear, 12, 31) }, ct);
        for (var month = 1; month <= 12; month++)
        {
            var periodName = $"P{month:00}";
            await c.EnsureAsync<FiscalCalendarPeriod>(x => x.FiscalCalendarYear == year.RecId && x.Name == periodName,
                new() { FiscalCalendar = cal.RecId, FiscalCalendarYear = year.RecId, Name = periodName, ShortName = periodName, Type = FiscalPeriodType.Operating,
                    StartDate = new(c.FiscalYear, month, 1), EndDate = new(c.FiscalYear, month, DateTime.DaysInMonth(c.FiscalYear, month)), Month = month, Quarter = (month - 1) / 3 + 1 }, ct);
        }
    }
}
