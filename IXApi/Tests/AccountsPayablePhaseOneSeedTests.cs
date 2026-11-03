using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

public sealed class AccountsPayablePhaseOneSeedTests
{
    [Fact]
    public void Registration_is_complete_idempotent_and_has_no_transaction_seeders()
    {
        var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().AddAccountsPayablePhaseOneSeeding();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var ordered = new SeedDependencyResolver().Resolve(scope.ServiceProvider.GetServices<IDataSeeder>());
        Assert.Equal(45, ordered.Count);
        Assert.DoesNotContain(ordered, x => x.Id.Contains("Cycle") || x.Id.Contains("PurchTable"));
        foreach (var seeder in ordered)
            foreach (var dependency in seeder.Dependencies)
                Assert.True(ordered.ToList().FindIndex(x => x.Id == dependency) < ordered.ToList().IndexOf(seeder));
    }

    [Fact]
    public void Dependency_errors_are_reported_before_execution()
    {
        Assert.Throws<InvalidOperationException>(() => new SeedDependencyResolver().Resolve([new Stub("A", ["B"])]));
        Assert.Throws<InvalidOperationException>(() => new SeedDependencyResolver().Resolve([new Stub("A", ["B"]), new Stub("B", ["A"])]));
    }

    [Fact]
    public async Task Phase_one_runs_twice_without_duplicates_and_preserves_sequence_progress()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var companyParty = new DirPartyTable { PartyNumber = "APV2-TEST-LEGAL", Name = "Test legal entity", IsActive = IAX.IXApi.Modules.Finance.Common.NoYes.Yes };
        db.Add(companyParty);
        await db.SaveChangesAsync();
        db.Set<CompanyInfo>().Add(new() { DataArea = "dat", Name = "AP phase-one test", DataAreaId = "dat", Party = companyParty.RecId });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        using var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
        using var scope = services.CreateScope();
        var runner = new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>());
        var first = await runner.RunAsync(2026, "phase-one-test");
        Assert.Equal(45, first.CompletedSeeders.Count);
        Assert.Equal(2, await db.Set<VendTable>().CountAsync());
        Assert.Equal(2, await db.Set<VendBankAccount>().CountAsync());
        var vendor = await db.Set<VendTable>().SingleAsync(x => x.Currency == "SAR");
        var party = await db.Set<DirPartyTable>().SingleAsync(x => x.RecId == vendor.Party);
        Assert.NotNull(party.PrimaryAddressLocation);
        Assert.NotNull(party.PrimaryContactEmail);
        var item = await db.Set<InventTable>().SingleAsync(x => x.ItemId == "APV2-STOCK");
        Assert.True(item.Product > 0);
        Assert.Equal(3, await db.Set<InventTableModule>().CountAsync(x => x.ItemId == item.ItemId));
        Assert.Single(await db.Set<InventModelGroupItem>().Where(x => x.ItemId == item.ItemId).ToListAsync());
        var counts = await CountsAsync(db);
        var tableCounts = counts.Split(';').Select(x => x.Split(':')).ToDictionary(x => x[0], x => long.Parse(x[1]));
        var requiredTables = "VendParameters VendGroup VendLedgerAccounts PaymTerm VendPaymModeTable CashDisc LedgerJournalName TaxTable TaxGroupHeading TaxGroupData TaxItemGroupHeading TaxOnItem Ledger Currency ExchangeRateType ExchangeRateCurrencyPair ExchangeRate MainAccount DimensionAttributeValueCombination InventPosting InventModelGroup InventParameters VendTable DirPartyTable DirPartyLocation LogisticsLocation LogisticsPostalAddress LogisticsElectronicAddress VendBankAccount BankAccountTable EcoResProduct InventTable InventTableModule InventSite InventLocation InventDim EcoResCategory PriceDiscTable".Split(' ');
        foreach (var table in requiredTables)
            Assert.True(tableCounts.GetValueOrDefault(table) > 0, $"Phase-one table {table} was not populated.");
        foreach (var table in new[] { "PurchTable", "VendTrans", "InventTrans", "GeneralJournalEntry", "BankAccountTrans" })
            Assert.Equal(0, tableCounts[table]);
        var sequence = await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().FirstAsync();
        sequence.NextRec = 12;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await runner.RunAsync(2026, "phase-one-test");
        Assert.Equal(counts, await CountsAsync(db));
        Assert.Equal(12, (await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().SingleAsync(x => x.RecId == sequence.RecId)).NextRec);

        var group = await db.Set<VendGroup>().SingleAsync(x => x.VendGroupCode == "APV2");
        group.Name = "Customized by operator";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2026, "phase-one-test"));
        Assert.Contains("Conflicting VendGroup.Name", exception.Message);
        Assert.Equal("Customized by operator", (await db.Set<VendGroup>().SingleAsync()).Name);
        Assert.Equal(counts, await CountsAsync(db));
    }

    private static async Task<string> CountsAsync(ApplicationDbContext db)
    {
        var counts = new List<string>();
        foreach (var table in db.Model.GetEntityTypes().Select(x => x.GetTableName()).OfType<string>().Distinct().Order())
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM \"" + table.Replace("\"", "\"\"") + "\"";
            counts.Add(table + ":" + await command.ExecuteScalarAsync());
        }
        return string.Join(";", counts);
    }

    private sealed class Stub(string id, string[] dependencies) : IDataSeeder
    {
        public string Id => id;
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public Task SeedAsync(SeedContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class Fixture(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Execute the production relationships in SQLite; normalize SQL Server storage types only.
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
