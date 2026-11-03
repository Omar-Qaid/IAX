using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples;

internal sealed class ErpCycleExampleContext : IAsyncDisposable
{
    internal ApplicationDbContext Db { get; }
    internal SalesScenarioSeeder.ExampleActor Actor { get; }
    internal UnitOfWork Work { get; }
    internal SysNumberSequenceService Numbers { get; }
    internal SalesInventoryNumberService InventoryNumbers { get; }
    internal InventDimensionResolver Dimensions { get; }
    internal DateTime Date { get; } = DateTime.UtcNow.Date;

    private ErpCycleExampleContext(ApplicationDbContext db, SalesScenarioSeeder.ExampleActor actor)
    {
        Db = db;
        Actor = actor;
        Work = new UnitOfWork(db);
        Numbers = new SysNumberSequenceService(Work, actor);
        InventoryNumbers = new SalesInventoryNumberService(Numbers);
        Dimensions = new InventDimensionResolver(Work, InventoryNumbers);
    }

    internal static async Task<ErpCycleExampleContext> CreateAsync(SeedingContext context, CancellationToken ct)
    {
        var user = await context.Db.Set<AspNetUser>().IgnoreQueryFilters()
            .Where(x => x.UserName == "sys").Select(x => x.Id).SingleAsync(ct);
        var actor = new SalesScenarioSeeder.ExampleActor(user);
        return new ErpCycleExampleContext(FinanceScenarioSupport.CreatePostingDb(context, actor), actor);
    }

    internal static void Require(int status, string? error)
    {
        if (status is < 200 or >= 300)
            throw new InvalidOperationException($"ERP cycle example failed ({status}): {error}");
    }

    public ValueTask DisposeAsync() => Db.DisposeAsync();
}
