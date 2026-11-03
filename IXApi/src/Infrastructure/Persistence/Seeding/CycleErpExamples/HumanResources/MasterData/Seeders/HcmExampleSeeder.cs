using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources;
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

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Company-scoped examples, separate from imported employee records.</summary>
public sealed  class HcmExampleSeeder : IAX.IXApi.Infrastructure.Persistence.Seeding.Core.BaseModuleSeeder
{
    public override string Id => "Finance.Foundation.HcmExamples";
    public override int Version => 1;
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory Category => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory.Demo;
    public override System.Collections.Generic.IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.Structure" };
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy Policy => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy.Repeatable;
    public override string Scope => "dat";

    public override System.Threading.Tasks.Task SeedAsync(IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeedingContext context, System.Threading.CancellationToken cancellationToken)
    {
        return SeedAsync(context.Db, context.Roles!, context.Users!, cancellationToken);
    }
    public const string Company = "dat";
    public static readonly DateOnly EffectiveFrom = new(2026, 1, 1);

    public override Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users, CancellationToken ct) => SeedExamplesAsync(db, ct);

    public Task SeedExamplesAsync(DbContext db, CancellationToken ct = default) =>
        new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources.HcmExampleService().SeedExamplesAsync(db, ct);


}
