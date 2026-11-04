using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;

/// <summary>Prepare existing module-owned configuration/master data and return its ordered report.</summary>
public sealed class AccountsPayableCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<ScenarioExecutionReport> PrepareAsync(AccountsPayableExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<AccountsPayableCycleValidator>().SingleOrDefault()
            ?? throw new InvalidOperationException("Register AddAccountsPayableConfigurationAndMasterDataExamples first.");
        validator.ValidateDefinition(); // Reject stale/malformed embedded manifests before writes.
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, validator.PreparationSteps.ToArray(),
            result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray(),
            validator.TakePreparedReport());
    }
}
