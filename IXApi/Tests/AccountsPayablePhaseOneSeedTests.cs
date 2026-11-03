using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

[CollectionDefinition("AccountsPayable phase one", DisableParallelization = true)]
public sealed class AccountsPayablePhaseOneSeedCollection { }

[Collection("AccountsPayable phase one")]
public sealed class AccountsPayablePhaseOneSeedTests
{
    [Fact]
    public void Registration_is_complete_idempotent_and_has_no_transaction_seeders()
    {
        var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().AddAccountsPayablePhaseOneSeeding();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var ordered = new SeedDependencyResolver().Resolve(scope.ServiceProvider.GetServices<IDataSeeder>());
        Assert.Equal(46, ordered.Count);
        Assert.Equal(2, scope.ServiceProvider.GetServices<ISeedValidator>().Count());
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
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        // Preserve production constraints while normalizing SQL Server Unicode literals for SQLite.
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        var companyParty = new DirPartyTable { PartyNumber = "APV2-TEST-LEGAL", Name = "Test legal entity", IsActive = IAX.IXApi.Modules.Finance.Common.NoYes.Yes };
        db.Add(companyParty);
        await db.SaveChangesAsync();
        db.Set<CompanyInfo>().Add(new() { DataArea = "dat", Name = "AP phase-one test", DataAreaId = "dat", Party = companyParty.RecId });
        db.Set<LogisticsLocation>().Add(new() { DataAreaId = "oth", LocationId = "OTHER-LOCATION", Description = "Other company - preserve" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        using var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
        using var scope = services.CreateScope();
        var runner = new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>());
        var first = await runner.RunAsync(2026, "phase-one-test");
        Assert.Equal(46, first.CompletedSeeders.Count);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SeedContext(db, 2026, "phase-one-test")
            .RequireAsync<LogisticsLocation>(x => x.LocationId == "OTHER-LOCATION", default));
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
        await AssertBusinessExampleAsync(db);
        var counts = await CountsAsync(db);
        var tableCounts = counts.Split(';').Select(x => x.Split(':')).ToDictionary(x => x[0], x => long.Parse(x[1]));
        var requiredTables = "VendParameters VendGroup VendLedgerAccounts PaymTerm VendPaymModeTable CashDisc LedgerJournalName TaxTable TaxGroupHeading TaxGroupData TaxItemGroupHeading TaxOnItem Ledger Currency ExchangeRateType ExchangeRateCurrencyPair ExchangeRate MainAccount DimensionAttributeValueCombination InventPosting InventModelGroup InventParameters VendTable DirPartyTable DirPartyLocation LogisticsLocation LogisticsPostalAddress LogisticsElectronicAddress VendBankAccount BankAccountTable EcoResProduct InventTable InventTableModule InventSite InventLocation InventDim EcoResCategory PriceDiscTable".Split(' ');
        foreach (var table in requiredTables)
            Assert.True(tableCounts.GetValueOrDefault(table) > 0, $"Phase-one table {table} was not populated.");
        foreach (var table in new[] { "PurchTable", "VendTrans", "InventTrans", "GeneralJournalEntry", "BankAccountTrans" })
            Assert.Equal(0, tableCounts[table]);
        var sequence = await db.Set<IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequence>().FirstAsync();
        var formattedOne = IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequenceService.FormatCode(sequence, 1);
        var formattedTwo = IAX.IXApi.Modules.Administration.NumberSequences.SysNumberSequenceService.FormatCode(sequence, 2);
        Assert.NotEqual(formattedOne, formattedTwo);
        Assert.DoesNotContain("#", formattedOne);
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
        // The next year creates calendar/rate rows before the vendor conflict; all must roll back.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2027, "phase-one-test"));
        Assert.Contains("Conflicting VendGroup.Name", exception.Message);
        Assert.Equal("Customized by operator", (await db.Set<VendGroup>().SingleAsync()).Name);
        Assert.Equal(counts, await CountsAsync(db));
    }

    [Theory]
    [InlineData("price")]
    [InlineData("company")]
    public async Task Commit_validator_rejects_semantically_invalid_references_and_rolls_back(string corruption)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        await InitializeCompanyAsync(db);
        var before = await CountsAsync(db);
        using var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
        using var scope = services.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var runner = new SeedRunner(db, seeders.Append(new CorruptExampleSeeder(seeders.Select(x => x.Id).ToArray(), corruption)),
            scope.ServiceProvider.GetServices<ISeedValidator>());
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2026, "integrity-test"));
        Assert.Contains(corruption == "price" ? "vendor price currency/unit/amount" : "DimensionAttributeValueCombination reference in dat", exception.Message);
        Assert.Equal(before, await CountsAsync(db));
    }

    [Fact]
    public async Task Mapped_string_lengths_are_checked_before_insert_even_on_SQLite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        var max = db.Model.FindEntityType(typeof(LogisticsLocation))!.FindProperty(nameof(LogisticsLocation.LocationId))!.GetMaxLength()!.Value;
        var context = new SeedContext(db, 2026, "integrity-test");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.EnsureAsync<LogisticsLocation>(x => x.LocationId == "TOO-LONG",
            new() { LocationId = new string('X', max + 1) }, default));
        Assert.Contains("LogisticsLocation.LocationId exceeds", exception.Message);
        Assert.Empty(await db.Set<LogisticsLocation>().ToListAsync());
    }

    [SqlServerPhaseOneFact]
    public async Task SQL_Server_real_schema_first_run_rerun_and_example_are_consistent()
    {
        // A generated database name is mandatory: this test never targets an application database.
        var database = "IXApi_APV2_Integrity_" + Guid.NewGuid().ToString("N");
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")!,
            InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 30
        };
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options);
        try
        {
            await db.Database.EnsureCreatedAsync();
            await InitializeCompanyAsync(db);
            using var services = new ServiceCollection().AddAccountsPayablePhaseOneSeeding().BuildServiceProvider();
            using var scope = services.CreateScope();
            var runner = new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>());
            await runner.RunAsync(2026, "sql-integrity-test");
            await AssertBusinessExampleAsync(db);
            var vendorIds = await db.Set<VendTable>().OrderBy(x => x.AccountNum).Select(x => x.RecId).ToArrayAsync();
            var counts = await CountsAsync(db);
            db.ChangeTracker.Clear();
            await runner.RunAsync(2026, "sql-integrity-test");
            Assert.Equal(counts, await CountsAsync(db));
            Assert.Equal(vendorIds, await db.Set<VendTable>().OrderBy(x => x.AccountNum).Select(x => x.RecId).ToArrayAsync());
        }
        finally
        {
            Assert.StartsWith("IXApi_APV2_Integrity_", db.Database.GetDbConnection().Database);
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static async Task InitializeCompanyAsync(ApplicationDbContext db)
    {
        var party = new DirPartyTable { PartyNumber = "APV2-TEST-LEGAL", Name = "Riyadh Business Services (Demo)", IsActive = IAX.IXApi.Modules.Finance.Common.NoYes.Yes };
        db.Add(party);
        await db.SaveChangesAsync();
        db.Add(new CompanyInfo { DataArea = "dat", Name = party.Name, Party = party.RecId });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static async Task AssertBusinessExampleAsync(ApplicationDbContext db)
    {
        var local = await db.Set<VendTable>().SingleAsync(x => x.AccountNum == "APV2-dat-LOCAL");
        var party = await db.Set<DirPartyTable>().SingleAsync(x => x.RecId == local.Party);
        Assert.Equal(VendorSeedExample.Name("LOCAL"), party.Name);
        var prices = await db.Set<PriceDiscTable>().Where(x => x.AccountRelation == local.AccountNum).ToListAsync();
        var paper = prices.Single(x => x.ItemRelation == InventorySeedExample.StockItem);
        var maintenance = prices.Single(x => x.ItemRelation == InventorySeedExample.ServiceItem);
        var net = 100m * paper.Amount / paper.PriceUnit + 2m * maintenance.Amount / maintenance.PriceUnit;
        var rate = await db.Set<TaxData>().SingleAsync(x => x.TaxCode == "APV2-15");
        var tax = decimal.Round(net * rate.TaxValue / 100m, 2);
        Assert.Equal(3200m, net);
        Assert.Equal(480m, tax);
        Assert.Equal(3680m, net + tax);
        Assert.Equal("SAR", paper.Currency);
        Assert.Equal("SAR", maintenance.Currency);
        var terms = await db.Set<PaymTerm>().SingleAsync(x => x.PaymTermId == local.PaymTermId);
        Assert.Equal(new DateTime(2026, 5, 15), new DateTime(2026, 4, 15).AddDays(terms.NumOfDays));
    }

    private sealed class CorruptExampleSeeder(string[] dependencies, string corruption) : IDataSeeder
    {
        public string Id => "ZZ.CorruptExample";
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public async Task SeedAsync(SeedContext c, CancellationToken ct)
        {
            if (corruption == "price")
            {
                var price = await c.Rows<PriceDiscTable>().SingleAsync(x => x.AccountRelation == "APV2-dat-LOCAL" && x.ItemRelation == "APV2-STOCK", ct);
                price.Currency = "USD"; // An existing currency; a foreign key alone cannot catch this mismatch.
            }
            else
            {
                var account = await c.Rows<MainAccount>().FirstAsync(ct);
                var combination = new DimensionAttributeValueCombination { DataAreaId = "oth", MainAccount = account.RecId, MainAccountValue = account.MainAccountId, DisplayValue = "OTHER-COMPANY" };
                c.Db.Add(combination);
                await c.Db.SaveChangesAsync(ct);
                var bank = await c.Rows<BankAccountTable>().SingleAsync(x => x.AccountId == "APV2-BANK", ct);
                bank.LedgerDimension = combination.RecId;
            }
            await c.Db.SaveChangesAsync(ct);
        }
    }

    private static async Task<string> CountsAsync(ApplicationDbContext db)
    {
        var counts = new List<string>();
        var opened = db.Database.GetDbConnection().State != System.Data.ConnectionState.Open;
        if (opened) await db.Database.OpenConnectionAsync();
        try
        {
        foreach (var table in db.Model.GetEntityTypes().Select(x => x.GetTableName()).OfType<string>().Distinct().Order())
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM \"" + table.Replace("\"", "\"\"") + "\"";
            counts.Add(table + ":" + await command.ExecuteScalarAsync());
        }
        return string.Join(";", counts);
        }
        finally
        {
            if (opened) await db.Database.CloseConnectionAsync();
        }
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

public sealed class SqlServerPhaseOneFactAttribute : FactAttribute
{
    public SqlServerPhaseOneFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")))
            Skip = "Set IXAPI_AP_PHASE_ONE_SQLSERVER to opt into a disposable local SQL Server database.";
    }
}
