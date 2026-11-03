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
    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        var db = context.Db;
        // Use the existing control-to-variable mappings and actual example request values.
        var values = await (from request in db.Set<WfRequest>().IgnoreQueryFilters()
                            join detail in db.Set<WfRequestDetail>().IgnoreQueryFilters() on request.RecId equals detail.RequestId
                            join map in db.Set<WfRequestMappingVariable>().IgnoreQueryFilters() on detail.ControlDataId equals (long?)map.RequestControlId
                            join variable in db.Set<WfVariable>().IgnoreQueryFilters() on map.VariableId equals variable.RecId
                            where request.DataAreaId == "dat" && request.Code.StartsWith("SEED-") && variable.ProcessId == request.ProcessId
                            select new { RequestId = request.RecId, VariableId = variable.RecId, detail.ControlValue }).ToListAsync(ct);
        foreach (var value in values.DistinctBy(x => new { x.RequestId, x.VariableId }))
            if (!await db.Set<WfRequestVariable>().IgnoreQueryFilters().AnyAsync(x => x.RequestId == value.RequestId && x.VariableId == value.VariableId, ct))
            {
                db.Add(new WfRequestVariable { RequestId = value.RequestId, VariableId = value.VariableId,
                    VariableValue = value.ControlValue, DataAreaId = "dat", CreatedBy = "sys", OwnerAccountId = "sys" });
                await db.SaveChangesAsync(ct);
            }
    }
}
