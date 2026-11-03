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

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources;

internal static class HcmShowroomSeeder
{
    internal static Task<HcmShowroom> SeedAsync(DbContext db, long party, string number, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.Party == party, HcmShowroomDemoData.Create(party, number), ct);
}
