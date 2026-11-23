using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
public sealed class FoundationCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
 public async Task<FoundationPreparationReport> PrepareAsync(FoundationExampleOptions options, CancellationToken ct = default)
 {
  ArgumentNullException.ThrowIfNull(options);
  var validator = validators.OfType<FoundationCycleValidator>().Single();
  validator.ValidateDefinition();
  var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
  return new(result.Company, options.FiscalYear, DateTime.UtcNow, result.CompletedSeeders.ToArray(), FoundationTableCatalog.MasterTypes.OrderBy(x => x).ToArray()) { MasterData = validator.TakePreparedReport(), PreparationSteps = validator.PreparationSteps.ToArray(),
   SeederSteps = result.CompletedSeeders.Select((id, index) => new SeederPreparationStep(index + 1, id)).ToArray() };
 }
}
