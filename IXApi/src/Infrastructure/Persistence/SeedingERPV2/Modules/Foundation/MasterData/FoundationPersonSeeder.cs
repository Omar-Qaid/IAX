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
public sealed class FoundationPersonSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.FoundationPersonSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var suffix in new[] { "MANAGER", "BUYER", "SHOWROOM" })
            await c.EnsureAsync<DirPartyTable>(x => x.PartyNumber == $"FNV2-{c.Company}-{suffix}", new() { PartyNumber = $"FNV2-{c.Company}-{suffix}", Name = "Foundation Demo " + suffix, IsActive = NoYes.Yes }, ct);
    }
}
