using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>The startup (baseline) profile seeds every module's reference data once and adds nothing on a rerun.</summary>
[Collection(SeedingCollection.Name)]
public sealed class BaselineSeedingTests
{
    [Fact]
    public async Task Baseline_profile_seeds_reference_data_and_a_rerun_adds_no_rows()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var configuration = new ConfigurationBuilder().Build();
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db).AddLogging()
            .AddSingleton<IConfiguration>(configuration).AddBaselineSeeding(configuration).BuildServiceProvider();

        var first = await Run(provider);
        Assert.Contains("Finance.Inventory.Setup", first.CompletedSeeders);
        Assert.Equal(provider.CreateScope().ServiceProvider.GetServices<IBaselineSeeder>()
            .Count(x => x.Kind == BaselineSeederKind.Repeatable), first.CompletedSeeders.Count);

        Assert.Equal(2, await db.Set<CompanyInfo>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(24, await db.Set<MainAccount>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(14, await db.Set<FiscalCalendarPeriod>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(14, await db.Set<LedgerFiscalCalendarPeriod>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(2, await db.Set<DimensionHierarchy>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(6, await db.Set<TaxTable>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(13, await db.Set<InventPosting>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(9, await db.Set<InventLocation>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(15, await db.Set<CustLedgerAccounts>().IgnoreQueryFilters().CountAsync());
        Assert.Equal(14, await db.Set<HcmPosition>().IgnoreQueryFilters().CountAsync());

        var before = await Counts(db);
        db.ChangeTracker.Clear();
        await Run(provider);
        var after = await Counts(db);
        Assert.Empty(after.Where(x => before[x.Key] != x.Value).Select(x => $"{x.Key}: {before[x.Key]} -> {x.Value}"));
    }

    [Fact]
    public async Task Legacy_imports_run_once_from_the_embedded_export_and_are_not_repeated()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seeding:EnableLegacyImports"] = "true", ["Seeding:EnableEmbeddedFixtures"] = "true"
        }).Build();
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db).AddLogging()
            .AddSingleton<IConfiguration>(configuration).AddBaselineSeeding(configuration).BuildServiceProvider();

        var first = await Run(provider);
        Assert.Contains("Finance.Foundation.LegacyEmployees", first.CompletedSeeders);
        Assert.Contains("Workflow.LegacyMaster", first.CompletedSeeders);
        Assert.True(await db.Set<IAX.IXApi.Modules.Finance.Foundation.HcmWorkers.HcmWorker>().IgnoreQueryFilters().AnyAsync());

        db.ChangeTracker.Clear();
        var second = await Run(provider);
        Assert.DoesNotContain("Finance.Foundation.LegacyEmployees", second.CompletedSeeders);
        Assert.DoesNotContain("Workflow.LegacyMaster", second.CompletedSeeders);
    }

    private static async Task<IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models.SeedResult> Run(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BaselineSeedRunner>().RunAsync();
    }

    /// <summary>Row count of every mapped table except the run history, which a rerun is allowed to extend.</summary>
    private static async Task<Dictionary<string, int>> Counts(ApplicationDbContext db)
    {
        var counts = new Dictionary<string, int>();
        foreach (var table in db.Model.GetEntityTypes().Select(x => x.GetTableName()).OfType<string>().Distinct())
        {
            if (table == "SysDataSeedLogs") continue;
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM \"{table}\"";
            counts[table] = Convert.ToInt32(await command.ExecuteScalarAsync());
        }
        return counts;
    }

}
