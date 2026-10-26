using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Seeds reusable workflow master data and a catalog of example request forms.
/// Runtime requests and history are intentionally not seeded. Add new simple
/// forms to the form catalog instead of duplicating seeding logic.
/// </summary>
public sealed partial  class WfProcessSeedData : IAX.IXApi.Infrastructure.Persistence.Seeding.Core.BaseModuleSeeder
{
    public override string Id => "Workflow.ProcessMaster";
    public override int Version => 1;
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory Category => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory.Required;
    public override System.Collections.Generic.IReadOnlyCollection<string> DependsOn => new[] { "Identity.MasterData" };
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy Policy => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy.Repeatable;
    public override string Scope => "global";

    public override System.Threading.Tasks.Task SeedAsync(IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeedingContext context, System.Threading.CancellationToken cancellationToken)
    {
        return SeedAsync(context.Db, context.Roles!, context.Users!, cancellationToken);
    }
    private const string ProcessCode = "PAYMENT_REQUEST";
    private const string RequiredRule = "{\"validations\":[{\"type\":\"required\",\"active\":true}]}";

    public async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users,
        CancellationToken ct)
    {
        _ = roles;
        var owner = (await users.FindByNameAsync("sys"))?.Id ?? "sys";

        await SeedMasterDataAsync(db, owner, ct);
        await SeedPaymentRequestExampleAsync(db, owner, ct);
        await SeedPerformersAsync(db, owner, ct);
        await SeedPerformerUsersAsync(db, owner, ct);

        await WfProcess590SeedData.SeedAsync(db, owner, ct);
        await WfProcess603SeedData.SeedAsync(db, owner, ct);
        await WfProcess651SeedData.SeedAsync(db, owner, ct);

        foreach (var form in AdditionalForms())
            await SeedFormAsync(db, form, owner, ct);
    }
}
