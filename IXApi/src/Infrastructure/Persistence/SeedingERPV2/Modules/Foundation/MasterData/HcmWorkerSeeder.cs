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
public sealed class HcmWorkerSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.HcmWorkerSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.FoundationPersonSeeder", "ERPV2.Foundation.GenderSeeder", "ERPV2.Foundation.HcmNationalitySeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var gender = await c.Db.Set<IAX.IXApi.Modules.Finance.Foundation.Genders.Gender>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == c.Company && x.Code == "FNV2", ct);
        var nationality = await c.Db.Set<IAX.IXApi.Modules.Finance.Foundation.Nationalities.HcmNationality>().IgnoreQueryFilters().SingleAsync(x => x.DataAreaId == c.Company && x.Code == "FNV2", ct);
        foreach (var suffix in new[] { "MANAGER", "BUYER" }) {
            var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == $"FNV2-{c.Company}-{suffix}", ct);
            await c.EnsureAsync<HcmWorker>(x => x.PersonnelNumber == "FNV2-" + suffix, new() { PersonnelNumber = "FNV2-" + suffix, Person = party.RecId, GenderId = gender.RecId, NationalityId = nationality.RecId, HireDate = new(2026,1,1) }, ct);
        }
    }
}
