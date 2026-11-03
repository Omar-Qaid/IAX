using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Workflow;

internal static class WfRequestVariableSeeder
{
    internal static Task<WfRequestVariable> SeedAsync(DbContext db, long request, long variable, string? value, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.RequestId == request && x.VariableId == variable, WfRequestVariableDemoData.Create(request, variable, value), ct);
}
