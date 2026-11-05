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
public sealed class DirPartyLocationRoleSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.DirPartyLocationRoleSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.LogisticsLocationRoleSeeder", "ERPV2.DirPartyLocationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var role = await c.RequireAsync<LogisticsLocationRole>(x => x.Name == "FNV2-Invoice", ct);
        foreach (var suffix in new[] { "LOCAL", "USD" }) {
            var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == $"APV2-{c.Company}-{suffix}", ct);
            var location = await c.RequireAsync<DirPartyLocation>(x => x.Party == party.RecId, ct);
            await c.EnsureAsync<DirPartyLocationRole>(x => x.PartyLocation == location.RecId && x.LocationRole == role.RecId, new() { PartyLocation = location.RecId, LocationRole = role.RecId }, ct);
        }
    }
}
