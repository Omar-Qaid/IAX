using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Before this, every ledger account combination was main-account-only, so there were no dimension
/// values for an account structure to validate. The resolver is what gives a combination its
/// segments, in both representations D365 keeps.
/// </summary>
public class LedgerDimensionResolverTests
{
    [Fact]
    public async Task Resolving_a_combination_writes_the_value_group_chain_and_the_named_column()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var account = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "611000");

        var combination = await new LedgerDimensionResolver(db)
            .ResolveAsync("dat", account, [("Department", "ADMIN")], accountStructure: 10, default);

        Assert.Equal("611000-ADMIN", combination.DisplayValue);
        Assert.Equal(account.RecId, combination.MainAccount);
        Assert.Equal("611000", combination.MainAccountValue);
        Assert.Equal(10, combination.AccountStructure);
        // The denormalised per-dimension column D365 also keeps.
        Assert.Equal("ADMIN", combination.DepartmentValue);
        Assert.NotEqual(0, combination.Department);

        db.ChangeTracker.Clear();
        var group = await db.Set<DimensionAttributeValueGroup>().AsNoTracking().SingleAsync();
        Assert.Equal(1, group.Levels);
        Assert.Equal(10, group.DimensionHierarchy);
        var level = await db.Set<DimensionAttributeLevelValue>().AsNoTracking().SingleAsync();
        Assert.Equal("ADMIN", level.DisplayValue);
        Assert.Equal(1, level.Ordinal);
        var link = await db.Set<DimensionAttributeValueGroupCombination>().AsNoTracking().SingleAsync();
        Assert.Equal(combination.RecId, link.DimensionAttributeValueCombination);
        Assert.Equal(group.RecId, link.DimensionAttributeValueGroup);
    }

    [Fact]
    public async Task Resolving_the_same_account_and_values_twice_reuses_the_combination()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var account = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "611000");
        var resolver = new LedgerDimensionResolver(db);

        var first = await resolver.ResolveAsync("dat", account, [("Department", "ADMIN")], 10, default);
        db.ChangeTracker.Clear();
        var second = await resolver.ResolveAsync("dat", account, [("Department", "ADMIN")], 10, default);

        Assert.Equal(first.RecId, second.RecId);
        Assert.Single(await db.Set<DimensionAttributeValueGroup>().AsNoTracking().ToListAsync());
    }

    // A blank segment is kept as a segment; it simply has no dimension value to link.
    [Fact]
    public async Task A_blank_segment_produces_a_bare_account_and_no_level_value()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var account = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "130100");

        var combination = await new LedgerDimensionResolver(db)
            .ResolveAsync("dat", account, [("Department", "")], 10, default);

        Assert.Equal("130100", combination.DisplayValue);
        Assert.Equal(string.Empty, combination.DepartmentValue);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<DimensionAttributeLevelValue>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Segment_values_read_back_in_level_order_with_their_dimension_names()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var account = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "611000");
        var resolver = new LedgerDimensionResolver(db);
        var combination = await resolver.ResolveAsync("dat", account, [("Department", "SALES")], 10, default);
        db.ChangeTracker.Clear();

        var segments = await resolver.SegmentValuesAsync("dat", combination.RecId, default);
        Assert.Equal([("Department", "SALES")], segments);
    }

    [Theory]
    [InlineData("Region", "ADMIN", "Financial dimension 'Region' does not exist")]
    [InlineData("Department", "LEGAL", "'LEGAL' is not a value of financial dimension 'Department'")]
    public async Task An_unknown_dimension_or_value_is_refused(string dimension, string value, string expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var account = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "611000");

        var error = await Assert.ThrowsAsync<LedgerDimensionException>(
            () => new LedgerDimensionResolver(db).ResolveAsync("dat", account, [(dimension, value)], 10, default));
        Assert.Contains(expected, error.Message);
    }

    [Fact]
    public async Task A_suspended_or_blocked_dimension_value_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var account = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "611000");
        var sales = await db.Set<DimensionAttributeValue>().SingleAsync(x => x.DisplayValue == "SALES");
        sales.IsSuspended = NoYes.Yes;
        var service = await db.Set<DimensionAttributeValue>().SingleAsync(x => x.DisplayValue == "SERVICE");
        service.IsBlockedForManualEntry = NoYes.Yes;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var resolver = new LedgerDimensionResolver(db);
        Assert.Contains("is suspended", (await Assert.ThrowsAsync<LedgerDimensionException>(
            () => resolver.ResolveAsync("dat", account, [("Department", "SALES")], 10, default))).Message);
        Assert.Contains("blocked for manual entry", (await Assert.ThrowsAsync<LedgerDimensionException>(
            () => resolver.ResolveAsync("dat", account, [("Department", "SERVICE")], 10, default))).Message);
    }

    private static async Task<DimensionContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new DimensionContext(new DbContextOptionsBuilder<DimensionContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "611000", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "130100", LedgerChartOfAccounts = 10 },
            new DimensionAttribute { RecId = 1, DataAreaId = "dat", Name = "Department" },
            new DimensionAttributeValue { RecId = 1, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "ADMIN" },
            new DimensionAttributeValue { RecId = 2, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "SALES" },
            new DimensionAttributeValue { RecId = 3, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "SERVICE" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class DimensionContext(DbContextOptions<DimensionContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(MainAccount), typeof(DimensionAttribute), typeof(DimensionAttributeValue),
                typeof(DimensionAttributeValueCombination), typeof(DimensionAttributeValueGroup),
                typeof(DimensionAttributeLevelValue), typeof(DimensionAttributeValueGroupCombination)];
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
            }
        }
    }
}
