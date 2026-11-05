using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public sealed class TaxReportPeriodSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.TaxReportPeriodSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.TaxPeriodHeadSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.RequireAsync<TaxPeriodHead>(x => x.TaxPeriod == "Monthly", ct);
        for (var month = 1; month <= 12; month++) {
            var from = new DateTime(c.FiscalYear, month, 1);
            await c.EnsureAsync<TaxReportPeriod>(x => x.TaxPeriod == "Monthly" && x.FromDate == from,
                new() { TaxPeriod = "Monthly", FromDate = from, ToDate = new(c.FiscalYear, month, DateTime.DaysInMonth(c.FiscalYear, month)), VersionNum = 0 }, ct);
        }
    }
}
