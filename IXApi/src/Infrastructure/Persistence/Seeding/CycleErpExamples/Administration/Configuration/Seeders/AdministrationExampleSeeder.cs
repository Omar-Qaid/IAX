using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed class AdministrationExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Administration.BatchExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override IReadOnlyCollection<string> DependsOn => new[] { "Administration.Settings" };
    public override Task SeedAsync(SeedingContext context, CancellationToken ct) =>
        new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Administration.AdministrationExampleService().SeedAsync(context, ct);
}
