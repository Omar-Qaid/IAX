using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;

/// <summary>Prepare existing module-owned configuration/master data and return its ordered report.</summary>
public sealed class AccountsReceivableCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<ScenarioExecutionReport> PrepareAsync(AccountsReceivableExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<AccountsReceivableCycleValidator>().SingleOrDefault()
            ?? throw new InvalidOperationException("Register AddAccountsReceivableConfigurationAndMasterDataExamples first.");
        validator.ValidateDefinition(); // Reject stale/malformed embedded manifests before writes.
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, validator.PreparationSteps.ToArray(),
            result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray(),
            validator.TakePreparedReport());
    }
}
