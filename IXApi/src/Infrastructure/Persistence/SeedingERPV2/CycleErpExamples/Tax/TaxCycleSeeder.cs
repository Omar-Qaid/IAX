using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;

public sealed class TaxCycleSeeder(SeedRunner runner)
{
    public Task<SeedResult> PrepareAsync(int fiscalYear, string actor, CancellationToken ct = default) =>
        runner.RunAsync(fiscalYear, actor, ct);
}
