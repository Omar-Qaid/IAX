using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IXApi.Tests;

public sealed class SeedingErpV2SqlServerFactAttribute : FactAttribute
{
    public SeedingErpV2SqlServerFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("IAX_SEED_ERP_V2_SQL_TESTS") != "1")
            Skip = "Set IAX_SEED_ERP_V2_SQL_TESTS=1 to create an isolated database on local SQL Server.";
    }
}

public sealed class SeedingErpV2SqlServerTests(ITestOutputHelper output)
{
    [SeedingErpV2SqlServerFact]
    public async Task All_module_setup_seeds_and_reruns_on_sql_server()
    {
        var database = $"IAX_SeedErpV2_{Guid.NewGuid():N}";
        output.WriteLine($"Disposable SQL Server fixture: {database}");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer($"Server=.;Database={database};Integrated Security=true;TrustServerCertificate=true",
                sql => sql.CommandTimeout(120))
            .Options;
        var actor = new SeedIdentity();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ICompanyExecutionContext>(actor)
            .AddSingleton<ICurrentUserService>(actor)
            .AddScoped(_ => new ApplicationDbContext(options, companyContext: actor))
            .AddGeneralLedgerTransactionExamples();
        using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.Database.EnsureCreatedAsync());

        var runner = scope.ServiceProvider.GetRequiredService<SeedRunner>();
        var first = await runner.RunAsync(2026, "seed-erp-v2-sql-test");
        var before = await ReadTableCountsAsync(db);
        db.ChangeTracker.Clear();
        var second = await runner.RunAsync(2026, "seed-erp-v2-sql-test");
        Assert.Equal(first.Company, second.Company);
        Assert.Equal(first.CompletedSeeders.ToArray(), second.CompletedSeeders.ToArray());
        Assert.Equal(before, await ReadTableCountsAsync(db));
        output.WriteLine($"Verified stable row counts across {before.Length} tables.");

        var examples = scope.ServiceProvider.GetRequiredService<GeneralLedgerTransactionExamples>();
        var journals = await examples.RunAsync(2026, "seed-erp-v2-sql-test");
        Assert.NotEqual(journals.Journal, journals.Reversal);
        var posted = await ReadTableCountsAsync(db);
        Assert.Equal(journals, await examples.RunAsync(2026, "seed-erp-v2-sql-test"));
        Assert.Equal(posted, await ReadTableCountsAsync(db));
    }

    private sealed class SeedIdentity : ICompanyExecutionContext, ICurrentUserService
    {
        public void ResetArea() { }
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "seed-erp-v2-sql-test";
        public string GetOwnerAccountId() => GetCurrentUserId();
    }

    private static async Task<(string Table, long Count)[]> ReadTableCountsAsync(ApplicationDbContext db)
    {
        const string query = """
            SELECT s.name + N'.' + t.name, SUM(p.rows)
            FROM sys.tables t
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
            GROUP BY s.name, t.name
            ORDER BY s.name, t.name
            """;
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            await using var reader = await command.ExecuteReaderAsync();
            var counts = new List<(string Table, long Count)>();
            while (await reader.ReadAsync())
                counts.Add((reader.GetString(0), reader.GetInt64(1)));
            return counts.ToArray();
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
