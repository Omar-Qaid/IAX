using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;

/// <summary>
/// Exercise both tax directions through the services that own the documents. Input tax comes from the
/// vendor journal examples and their accounting associations; output tax comes from the free text
/// invoice, which posts through its own service and moves no inventory. Nothing is inserted here.
/// </summary>
public sealed class TaxTransactionExamples(
    ApplicationDbContext db,
    TaxCycleSeeder preparation,
    FoundationTransactionExamples inputTax,
    AccountsReceivableTransactionExamples outputTax)
{
    /// <summary>The output-tax scenario: a free text invoice whose single line carries the demo tax group.</summary>
    private static readonly string[] OutputTaxScenarioIds = ["04"];

    public async Task<TaxTransactionReport> RunAsync(TaxExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var prepared = await preparation.PrepareAsync(options, ct);
        // Each step runs its own seed runner, which refuses a scope that still tracks entities.
        db.ChangeTracker.Clear();
        var incoming = await inputTax.RunAsync(new FoundationExampleOptions(options.FiscalYear, options.Actor), ct);
        db.ChangeTracker.Clear();
        var outgoing = await outputTax.RunAsync(new AccountsReceivableTransactionOptions(options.FiscalYear, options.Actor, OutputTaxScenarioIds), ct);
        return new(prepared.Company, options.FiscalYear, prepared, incoming, outgoing);
    }
}
