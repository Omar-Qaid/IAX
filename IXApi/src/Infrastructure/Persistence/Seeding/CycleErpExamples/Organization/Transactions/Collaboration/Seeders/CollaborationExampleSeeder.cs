using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Local demonstration records only; no delivery service or external channel is invoked.</summary>
public sealed class CollaborationExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Organization.CollaborationExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Identity.MasterData" };

    public override Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        => SeedExamplesAsync(context.Db, cancellationToken);

    public Task SeedExamplesAsync(DbContext db, CancellationToken ct = default) =>
        new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization.CollaborationExampleService().SeedExamplesAsync(db, ct);
}
