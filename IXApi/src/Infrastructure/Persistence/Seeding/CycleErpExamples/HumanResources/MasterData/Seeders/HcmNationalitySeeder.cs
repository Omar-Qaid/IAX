using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HcmGender = IAX.IXApi.Modules.Finance.Foundation.Genders.Gender;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources;

internal static class HcmNationalitySeeder
{
    internal static async Task<HcmNationality> SeedAsync(DbContext db, string code, string name, string alias, CancellationToken ct)
    {
        var entity = await db.Set<HcmNationality>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.Code == code, ct);
        if (entity == null) { entity = HcmNationalityDemoData.Create(code, name, alias); db.Add(entity); }
        else if (string.IsNullOrWhiteSpace(entity.NameAlias)) entity.NameAlias = alias;
        await db.SaveChangesAsync(ct);
        return entity;
    }
}
