using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed class DocumentExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Organization.DocumentExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] { "Organization.DocumentManagement", "Finance.AccountsReceivable.DemoCustVend" };
    public override Task SeedAsync(SeedingContext context, CancellationToken ct) => SeedExamplesAsync(context.Db, ct);

    public Task SeedExamplesAsync(DbContext db, CancellationToken ct = default) =>
        new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization.DocumentExampleService().SeedExamplesAsync(db, ct);
}
