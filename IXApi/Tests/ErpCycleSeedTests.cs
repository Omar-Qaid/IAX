using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

public sealed class ErpCycleSeedTests
{
    [Fact]
    public void Full_cycle_resolves_and_runs_after_its_module_dependencies_only_when_demo_is_enabled()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSeedingCore(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var seeders = provider.GetServices<IModuleSeeder>().ToArray();
        var cycle = Assert.Single(seeders.OfType<ErpCycleExampleService>());
        var graph = new SeederDependencyGraph();
        Assert.DoesNotContain(cycle, graph.GetExecutionOrder(seeders, new SeedingOptions()));
        var order = graph.GetExecutionOrder(seeders, new SeedingOptions { EnableDemoData = true }).ToList();
        Assert.All(cycle.DependsOn, dependency =>
            Assert.True(order.FindIndex(x => x.Id == dependency) < order.IndexOf(cycle)));
        Assert.Equal(SeederExecutionPolicy.RunOnce, cycle.Policy);
        Assert.Equal("dat", cycle.Scope);
    }

    [Fact]
    public async Task New_setup_is_company_scoped_repeatable_and_preserves_custom_names()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ExampleDb(new DbContextOptionsBuilder<ExampleDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await AddPrerequisitesAsync(db);
        db.Add(new VendPaymModeTable { DataAreaId = "other", PaymMode = "ERP-BANK", Name = "Other company", AccountType = LedgerJournalACType.Ledger });
        await db.SaveChangesAsync();
        var service = new FoundationCycleExampleService();
        await service.SeedDataAsync(db, "sys");
        var mode = await db.Set<VendPaymModeTable>().SingleAsync(x => x.DataAreaId == "dat");
        mode.Name = "Custom bank payment name";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await service.SeedDataAsync(db, "sys");
        Assert.Equal(2, await db.Set<VendPaymModeTable>().CountAsync());
        Assert.Equal("Custom bank payment name", (await db.Set<VendPaymModeTable>().SingleAsync(x => x.DataAreaId == "dat")).Name);
        Assert.Equal("Other company", (await db.Set<VendPaymModeTable>().SingleAsync(x => x.DataAreaId == "other")).Name);
    }

    [Fact]
    public async Task Conflicting_payment_setup_fails_without_overwriting_it()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ExampleDb(new DbContextOptionsBuilder<ExampleDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await AddPrerequisitesAsync(db);
        db.Add(new VendPaymModeTable { DataAreaId = "dat", PaymMode = "ERP-BANK", AccountType = LedgerJournalACType.Ledger, Name = "Custom ledger method" });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => new FoundationCycleExampleService().SeedDataAsync(db, "sys"));
        var mode = await db.Set<VendPaymModeTable>().SingleAsync();
        Assert.Equal(LedgerJournalACType.Ledger, mode.AccountType);
        Assert.Equal("Custom ledger method", mode.Name);
    }

    private static async Task AddPrerequisitesAsync(DbContext db)
    {
        db.Add(new CompanyInfo { DataArea = "dat", RowVersion = [] });
        foreach (var area in new[] { "dat", "other" })
        {
            db.Add(new CustTable { DataAreaId = area, AccountNum = "CUST-100" });
            db.Add(new VendTable { DataAreaId = area, AccountNum = "VEND-100" });
            db.Add(new InventTable { DataAreaId = area, ItemId = "ITEM-001" });
            db.Add(new Ledger { DataAreaId = area, AccountingCurrency = "SAR", ReportingCurrency = "SAR" });
        }
        await db.SaveChangesAsync();
    }

    private sealed class ExampleDb(DbContextOptions<ExampleDb> options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // SQLite does not generate SQL Server rowversion values.
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added
                         && x.Metadata.FindProperty("RowVersion") != null))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            var allowed = new[] { typeof(CompanyInfo), typeof(CustTable), typeof(VendTable), typeof(InventTable), typeof(Ledger), typeof(VendPaymModeTable) };
            foreach (var type in allowed) b.Entity(type);
            foreach (var type in b.Model.GetEntityTypes().Where(x => !allowed.Contains(x.ClrType)).Select(x => x.ClrType).ToList())
                b.Ignore(type);
            foreach (var type in allowed)
            {
                b.Entity(type).HasKey("RecId");
                if (b.Entity(type).Metadata.FindProperty("RowVersion") != null)
                    b.Entity(type).Ignore("RowVersion");
            }
            b.Entity<VendPaymModeTable>().HasIndex(x => new { x.DataAreaId, x.PaymMode }).IsUnique();
        }
    }
}
