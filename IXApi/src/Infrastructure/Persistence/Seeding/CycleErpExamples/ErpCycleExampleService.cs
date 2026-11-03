using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples;

/// <summary>One complete trading example, with module-owned service steps like the workflow process examples.</summary>
public sealed class ErpCycleExampleService(
    FoundationCycleExampleService foundation,
    PurchaseCycleExampleService purchases,
    SalesCycleExampleService sales,
    InventoryCycleExampleService inventory,
    LedgerCycleExampleService ledger) : BaseModuleSeeder
{
    public override string Id => "ERP.TradingCycleExample";
    public override SeederCategory Category => SeederCategory.Demo;
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.RunOnce;
    public override string Scope => ErpCycleExampleData.Company;
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.InventoryFullCycle", "Finance.DraftExamples" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        // The posting services own transactions. Durable example keys allow a retry after any committed step.
        await using var example = await ErpCycleExampleContext.CreateAsync(context, ct);
        await foundation.SeedAsync(example, ct);
        await purchases.SeedAsync(example, ct);
        await sales.SeedAsync(example, ct);
        await inventory.SeedAsync(example, ct);
        await ledger.SeedAsync(example, ct);
    }
}
