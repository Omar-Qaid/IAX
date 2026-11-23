using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>GL-02: the seeded fifteen-step reference cycle reaches the 6,300 trial balance and replays without posting again.</summary>
public sealed class GeneralLedgerReferenceCycleSeedTests
{
    [Fact]
    public async Task Reference_cycle_seeds_the_six_thousand_three_hundred_trial_balance_and_replays_without_new_postings()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        var actor = new TestIdentity("gl-test");
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddLogging().AddGeneralLedgerTransactionExamples().BuildServiceProvider();

        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<GeneralLedgerTransactionExamples>();
        var report = await examples.RunAsync(new GeneralLedgerTransactionOptions(2026, "gl-test", ["02"]));
        var scenario = Assert.Single(report.Scenarios);
        Assert.Equal("02", scenario.Id);
        Assert.Equal(GeneralLedgerReferenceCycleFlow.TrialBalance, scenario.Amount);
        var entries = await db.Set<GeneralJournalEntry>().CountAsync();
        var movements = await db.Set<InventTrans>().CountAsync();
        db.ChangeTracker.Clear();

        // The replay verifies every step again and must not post anything new.
        await examples.RunAsync(new GeneralLedgerTransactionOptions(2026, "gl-test", ["02"]));
        Assert.Equal(entries, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(movements, await db.Set<InventTrans>().CountAsync());
    }


}
