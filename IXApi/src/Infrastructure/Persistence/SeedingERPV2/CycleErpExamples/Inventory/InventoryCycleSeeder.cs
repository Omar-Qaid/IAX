using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;

/// <summary>Prepare existing module-owned configuration/master data and return its ordered report.</summary>
public sealed class InventoryCycleSeeder(SeedRunner runner, IEnumerable<ISeedValidator> validators)
{
    public async Task<InventoryPreparationReport> PrepareAsync(InventoryExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var validator = validators.OfType<InventoryCycleValidator>().SingleOrDefault()
            ?? throw new InvalidOperationException("Register AddInventoryConfigurationAndMasterDataExamples first.");
        validator.ValidateDefinition(); // Reject stale/malformed embedded manifests before writes.
        var result = await runner.RunAsync(options.FiscalYear, options.Actor, ct);
        return new(result.Company, options.FiscalYear, DateTime.UtcNow, validator.PreparationSteps.ToArray(),
            result.CompletedSeeders.Select((id, index) => new InventorySeederStep(index + 1, id)).ToArray(),
            validator.TakePreparedReport());
    }
}
