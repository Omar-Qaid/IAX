using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow;

/// <summary>
/// Configuration and master data only. A workflow request is raised and routed by the workflow
/// services as a person submits and approves it, so this module documents no transaction scenario.
/// </summary>
public sealed class WorkflowCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<WorkflowPreparationReport> PrepareAsync(WorkflowExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<WorkflowCycleValidator>().Single();
        validator.ValidateDefinition();
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, result.CompletedSeeders.ToArray(),
            WorkflowTableCatalog.MasterTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray())
        {
            MasterData = validator.TakePreparedReport(),
            PreparationSteps = validator.PreparationSteps.ToArray(),
            SeederSteps = result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray()
        };
    }
}
