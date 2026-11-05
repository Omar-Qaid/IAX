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
public sealed class HcmShowroomSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.HcmShowroomSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.FoundationPersonSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == $"FNV2-{c.Company}-SHOWROOM", ct);
        await c.EnsureAsync<HcmShowroom>(x => x.PersonnelNumber == "FNV2-SHOWROOM", new() { PersonnelNumber = "FNV2-SHOWROOM", Party = party.RecId }, ct);
    }
}
