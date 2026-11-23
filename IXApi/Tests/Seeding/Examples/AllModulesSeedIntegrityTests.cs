using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>All eleven modules' configuration and master data seeded together, then rerun.</summary>
public sealed class AllModulesSeedIntegrityTests
{
    private static readonly string[] Modules =
    [
        "Foundation", "GeneralLedger", "Tax", "CashAndBank", "Inventory", "AccountsPayable",
        "AccountsReceivable", "Administration", "Communication", "Workflow", "Organization",
    ];

    [Fact]
    public void Every_module_contributes_seeders_and_an_integrity_check()
    {
        using var provider = new ServiceCollection().AddAllModulesPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = Namespaces(scope.ServiceProvider.GetServices<IDataSeeder>().Select(x => x.GetType()));
        var validators = Namespaces(scope.ServiceProvider.GetServices<ISeedValidator>().Select(x => x.GetType()));
        foreach (var module in Modules)
            Assert.Contains(module, seeders);
        // Every module except GL and Inventory, whose references the AP/AR phase validators check, owns a validator.
        foreach (var module in Modules.Except(["GeneralLedger", "Inventory"]))
            Assert.Contains(module, validators);
    }

    [Fact]
    public async Task All_modules_seed_together_and_a_rerun_changes_nothing()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = new ServiceCollection().AddAllModulesPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var validators = scope.ServiceProvider.GetServices<ISeedValidator>().ToArray();

        var first = await new SeedRunner(db, seeders, validators).RunAsync(2026, "all-modules-test");
        var before = await CountsAsync(database.Connection);
        db.ChangeTracker.Clear();
        var second = await new SeedRunner(db, seeders, validators).RunAsync(2026, "all-modules-test");

        Assert.Equal(first.Company, second.Company);
        Assert.Equal(first.CompletedSeeders.ToArray(), second.CompletedSeeders.ToArray());
        Assert.Equal(seeders.Length, first.CompletedSeeders.Count);
        var after = await CountsAsync(database.Connection);
        // The run history is the only table a rerun may grow.
        Assert.Equal(before.Where(x => x.Key != "SysDataSeedLogs"), after.Where(x => x.Key != "SysDataSeedLogs"));
        Assert.True(before.Count(x => x.Value > 0) > 50, "Expected the combined run to populate the module tables.");
    }

    private static HashSet<string> Namespaces(IEnumerable<Type> types) =>
        types.Select(type => Modules.FirstOrDefault(module => type.Namespace!.Contains($".{module}")) ?? "")
            .ToHashSet();

    private static async Task<SortedDictionary<string, long>> CountsAsync(SqliteConnection connection)
    {
        var tables = new List<string>();
        await using (var list = connection.CreateCommand())
        {
            list.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'";
            await using var reader = await list.ExecuteReaderAsync();
            while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
        }
        var counts = new SortedDictionary<string, long>(StringComparer.Ordinal);
        foreach (var table in tables)
        {
            await using var count = connection.CreateCommand();
            count.CommandText = $"SELECT COUNT(*) FROM \"{table}\"";
            counts[table] = (long)(await count.ExecuteScalarAsync())!;
        }
        return counts;
    }

}
