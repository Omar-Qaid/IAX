using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Communication;

/// <summary>
/// Configuration and master data only. A notification is raised by the service that handles the event
/// and sent by the queued worker, so this module documents no transaction scenario.
/// </summary>
public sealed class CommunicationCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<CommunicationPreparationReport> PrepareAsync(CommunicationExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<CommunicationCycleValidator>().Single();
        validator.ValidateDefinition();
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, result.CompletedSeeders.ToArray(),
            CommunicationTableCatalog.MasterTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray())
        {
            MasterData = validator.TakePreparedReport(),
            PreparationSteps = validator.PreparationSteps.ToArray(),
            SeederSteps = result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray()
        };
    }
}
