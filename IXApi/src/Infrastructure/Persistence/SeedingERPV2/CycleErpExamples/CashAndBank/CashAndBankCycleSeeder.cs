using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank;

/// <summary>Configuration and master data only; no bank movement is posted here.</summary>
public sealed class CashAndBankCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<CashAndBankPreparationReport> PrepareAsync(CashAndBankExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<CashAndBankCycleValidator>().Single();
        validator.ValidateDefinition();
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, result.CompletedSeeders.ToArray(),
            CashAndBankTableCatalog.MasterTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray())
        {
            MasterData = validator.TakePreparedReport(),
            PreparationSteps = validator.PreparationSteps.ToArray(),
            SeederSteps = result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray()
        };
    }
}
