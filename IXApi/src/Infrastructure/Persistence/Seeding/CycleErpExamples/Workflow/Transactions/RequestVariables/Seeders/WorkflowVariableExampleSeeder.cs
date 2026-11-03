using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Workflow;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed class WorkflowVariableExampleSeeder : BaseModuleSeeder
{
    public override string Id => "Workflow.RequestVariableExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override IReadOnlyCollection<string> DependsOn => new[] { "Workflow.DemoRequestTracking" };
    public override Task SeedAsync(SeedingContext context, CancellationToken ct) =>
        new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Workflow.WorkflowVariableExampleService().SeedAsync(context, ct);
}
