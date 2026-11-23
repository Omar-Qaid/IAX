using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Administration.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Administration.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Administration.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Administration;

/// <summary>
/// Configuration and master data only. Run history is written by the batch runner and allocated codes
/// by the number sequence service as they execute, so this module documents no transaction scenario.
/// </summary>
public sealed class AdministrationCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<AdministrationPreparationReport> PrepareAsync(AdministrationExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<AdministrationCycleValidator>().Single();
        validator.ValidateDefinition();
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, result.CompletedSeeders.ToArray(),
            AdministrationTableCatalog.MasterTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray())
        {
            MasterData = validator.TakePreparedReport(),
            PreparationSteps = validator.PreparationSteps.ToArray(),
            SeederSteps = result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray()
        };
    }
}
