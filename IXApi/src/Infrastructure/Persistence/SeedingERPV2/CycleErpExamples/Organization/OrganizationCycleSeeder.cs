using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Organization;

/// <summary>
/// Configuration and master data only. An attachment is stored by the document service, which needs an
/// uploaded file and a record to hang it on, so this module documents no transaction scenario.
/// </summary>
public sealed class OrganizationCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<OrganizationPreparationReport> PrepareAsync(OrganizationExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<OrganizationCycleValidator>().Single();
        validator.ValidateDefinition();
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, result.CompletedSeeders.ToArray(),
            OrganizationTableCatalog.MasterTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray())
        {
            MasterData = validator.TakePreparedReport(),
            PreparationSteps = validator.PreparationSteps.ToArray(),
            SeederSteps = result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray()
        };
    }
}
