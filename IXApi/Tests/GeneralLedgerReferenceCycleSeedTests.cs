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

namespace IXApi.Tests;

/// <summary>GL-02: the seeded fifteen-step reference cycle reaches the 6,300 trial balance and replays without posting again.</summary>
public sealed class GeneralLedgerReferenceCycleSeedTests
{
    [Fact]
    public async Task Reference_cycle_seeds_the_six_thousand_three_hundred_trial_balance_and_replays_without_new_postings()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        var actor = new Identity();
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

    private sealed class Identity : ICompanyExecutionContext, ICurrentUserService
    {
        public void ResetArea() { }
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "gl-test";
        public string GetOwnerAccountId() => "gl-test";
    }

    private sealed class Fixture(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
            {
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                property.SetColumnType(type == typeof(byte[]) ? "BLOB" : type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid) ? "TEXT" : "INTEGER");
                property.SetDefaultValueSql(null);
                if (property.Name == "RowVersion") property.SetDefaultValueSql("X'00'");
            }
        }
    }
}
