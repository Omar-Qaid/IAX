using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Migrations;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Tax;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class TaxTransGeneralJournalAccountEntryTests
{
    [Fact]
    public void Migration_only_creates_the_association_table_and_its_indexes()
    {
        var migration = new AddTaxTransGeneralJournalAccountEntry();
        var table = Assert.Single(migration.UpOperations.OfType<CreateTableOperation>());
        Assert.Equal("TaxTransGeneralJournalAccountEntry", table.Name);
        Assert.Empty(table.ForeignKeys);
        Assert.Equal(4, migration.UpOperations.Count);
        Assert.All(migration.UpOperations.OfType<CreateIndexOperation>(), index => Assert.Equal(table.Name, index.Table));
        Assert.Equal(3, migration.UpOperations.OfType<CreateIndexOperation>().Count());
        Assert.Equal(table.Name, Assert.IsType<DropTableOperation>(Assert.Single(migration.DownOperations)).Name);
    }

    [Fact]
    public void SqlServer_model_uses_the_supplied_columns_and_generated_system_row_version()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true")
            .Options);
        var entity = db.Model.FindEntityType(typeof(TaxTransGeneralJournalAccountEntry))!;
        var table = StoreObjectIdentifier.Table("TaxTransGeneralJournalAccountEntry", null);
        var columns = entity.GetProperties().Select(property => property.GetColumnName(table)).ToHashSet();
        foreach (var column in new[] { "GeneralJournalAccountEntry", "LedgerDimension", "TaxTrans", "TaxTransRelationship", "TaxUncommitted", "AccountingCurrencyAmount", "MonetaryAmountType", "SubledgerJournalAccountEntry", "TransactionCurrency", "TransactionCurrencyAmount", "DataAreaId", "RecVersion", "Partition", "RECID", "AllocationFactor", "SysRowVersion", "SysDataStateCode" })
            Assert.Contains(column, columns);
        Assert.DoesNotContain("RowVersion", columns);
        var rowVersion = entity.FindProperty(nameof(TaxTransGeneralJournalAccountEntry.RowVersion))!;
        Assert.True(rowVersion.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);
        Assert.Equal("rowversion", rowVersion.GetColumnType());
        foreach (var name in new[] { "AccountingCurrencyAmount", "TransactionCurrencyAmount", "AllocationFactor" })
        {
            var property = entity.FindProperty(name)!;
            Assert.Equal(32, property.GetPrecision());
            Assert.Equal(6, property.GetScale());
        }
        Assert.Equal(typeof(long?), entity.FindProperty("TaxUncommitted")!.ClrType);
        Assert.Equal(typeof(int?), entity.FindProperty("TaxTransRelationship")!.ClrType);
        Assert.Equal(typeof(int?), entity.FindProperty("MonetaryAmountType")!.ClrType);
        Assert.Equal(4, entity.FindProperty("DataAreaId")!.GetMaxLength());
        Assert.Contains("GetDataAreaId", entity.GetQueryFilter()!.ToString());
        Assert.Contains(entity.GetIndexes(), index => index.Properties.Select(property => property.Name)
            .SequenceEqual(new[] { "DataAreaId", "TaxTrans" }));
    }

    [Fact]
    public async Task SQLite_roundtrip_preserves_zero_references_bigint_ids_and_six_decimal_places()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AssociationContext(new DbContextOptionsBuilder<AssociationContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var row = new TaxTransGeneralJournalAccountEntry
        {
            GeneralJournalAccountEntry = 5637144953L,
            LedgerDimension = 22565434801L,
            TaxTrans = 5637144576L,
            TaxTransRelationship = 2,
            TaxUncommitted = 0,
            AccountingCurrencyAmount = 123.456789m,
            MonetaryAmountType = 0,
            SubledgerJournalAccountEntry = 0,
            TransactionCurrency = "",
            TransactionCurrencyAmount = -0.000001m,
            AllocationFactor = 0.333333m,
            DataAreaId = "usmf",
            Partition = 5637144576L,
            RecVersion = 0,
            SysDataStateCode = 0,
            RowVersion = new byte[8]
        };
        db.Add(row);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var stored = await db.Set<TaxTransGeneralJournalAccountEntry>().SingleAsync();
        Assert.Equal(22565434801L, stored.LedgerDimension);
        Assert.Equal(5637144576L, stored.Partition);
        Assert.Equal(0L, stored.TaxUncommitted);
        Assert.Equal(0L, stored.SubledgerJournalAccountEntry);
        Assert.Equal(123.456789m, stored.AccountingCurrencyAmount);
        Assert.Equal(-0.000001m, stored.TransactionCurrencyAmount);
        Assert.Equal(0.333333m, stored.AllocationFactor);
        Assert.Equal("", stored.TransactionCurrency);
        Assert.Equal(0, stored.RecVersion);
    }

    private sealed class AssociationContext(DbContextOptions<AssociationContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TaxTransGeneralJournalAccountEntryConfiguration());
            // SQLite has no SQL Server rowversion generator; generation is checked in the SQL Server model test.
            modelBuilder.Entity<TaxTransGeneralJournalAccountEntry>().Property(row => row.RowVersion)
                .ValueGeneratedNever().IsConcurrencyToken(false);
        }
    }
}
