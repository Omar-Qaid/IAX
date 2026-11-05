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
public sealed class TaxPeriodHeadSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.TaxPeriodHeadSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.TaxAuthorityAddressSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.EnsureAsync<TaxPeriodHead>(x => x.TaxPeriod == "Monthly", new() { TaxPeriod = "Monthly", Name = "Foundation monthly demo period", TaxAuthority = "FNV2", QtyUnit = 1, PeriodUnit = TaxPeriodUnit.Month }, ct);
    }
}
