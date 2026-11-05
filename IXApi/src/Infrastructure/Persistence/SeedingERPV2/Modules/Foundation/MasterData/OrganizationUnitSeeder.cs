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
public sealed class OrganizationUnitSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.OrganizationUnitSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.EnsureAsync<OrganizationUnit>(x => x.Code == "FNV2-FIN", new() { Code = "FNV2-FIN", Name = "Foundation Finance", OrganizationUnitType = 8, ValidFrom = new(2026,1,1) }, ct, nameof(OrganizationUnit.Code), nameof(OrganizationUnit.Name), nameof(OrganizationUnit.OrganizationUnitType), nameof(OrganizationUnit.ValidFrom));
    }
}
