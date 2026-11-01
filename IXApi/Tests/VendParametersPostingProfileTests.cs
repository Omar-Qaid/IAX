using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The company's default vendor posting profile, from VendParameters.
///
/// Vendor summary account resolution used to consider every VendLedgerAccounts rule in the company,
/// whatever posting profile it belonged to, so a company with two profiles resolved whichever rule
/// happened to match first. The receivable side has always narrowed to the CustParameters default;
/// these tests pin the payable mirror.
/// </summary>
public class VendParametersPostingProfileTests
{
    [Fact]
    public async Task The_default_profile_decides_which_rule_resolves()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // Both rules match the vendor by group. Only the profile tells them apart.
        AddRule(db, 1, "GEN", AccountCode.Group, "VG-1", dimension: 3);
        AddRule(db, 2, "ALT", AccountCode.Group, "VG-1", dimension: 4);
        db.Add(new VendParameters { RecId = 1, DataAreaId = "dat", Key = 0, VendPostingProfile = "ALT" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var resolved = await ResolveAsync(db);

        Assert.Equal("210200", resolved.MainAccountValue);
    }

    [Fact]
    public async Task Table_still_beats_group_within_the_chosen_profile()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddRule(db, 1, "GEN", AccountCode.Group, "VG-1", dimension: 3);
        AddRule(db, 2, "GEN", AccountCode.Table, "V-001", dimension: 4);
        db.Add(new VendParameters { RecId = 1, DataAreaId = "dat", Key = 0, VendPostingProfile = "GEN" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var resolved = await ResolveAsync(db);

        Assert.Equal("210200", resolved.MainAccountValue);
    }

    /// <summary>
    /// The same fallback the customer path uses: a configured profile with no rules of its own does
    /// not strand posting, because narrowing to nothing would refuse a document that the company's
    /// remaining rules can still account for.
    /// </summary>
    [Fact]
    public async Task A_profile_with_no_rules_of_its_own_falls_back_to_every_rule()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddRule(db, 1, "GEN", AccountCode.All, "", dimension: 3);
        db.Add(new VendParameters { RecId = 1, DataAreaId = "dat", Key = 0, VendPostingProfile = "MISSING" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var resolved = await ResolveAsync(db);

        Assert.Equal("210100", resolved.MainAccountValue);
    }

    [Fact]
    public async Task With_no_parameters_row_resolution_is_unchanged()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddRule(db, 1, "GEN", AccountCode.All, "", dimension: 3);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var resolved = await ResolveAsync(db);

        Assert.Equal("210100", resolved.MainAccountValue);
    }

    [Fact]
    public async Task A_second_parameters_row_for_the_company_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Add(new VendParameters { RecId = 1, DataAreaId = "dat", Key = 0, VendPostingProfile = "GEN" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        db.Add(new VendParameters { RecId = 2, DataAreaId = "dat", Key = 0, VendPostingProfile = "ALT" });

        // The unique (DataAreaId, Key) index: a second row would make the default ambiguous.
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static async Task<DimensionAttributeValueCombination> ResolveAsync(DbContext db) =>
        await new SubledgerPostingSupport(db).ResolveVendorSummaryAsync(
            "dat", new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", VendGroup = "VG-1" },
            CancellationToken.None);

    private static void AddRule(DbContext db, long recId, string profile, AccountCode code, string num, long dimension) =>
        db.Add(new VendLedgerAccounts
        {
            RecId = recId, DataAreaId = "dat", PostingProfile = profile,
            AccountCode = code, Num = num, SummaryLedgerDimension = dimension
        });

    private static async Task<VendParametersContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new VendParametersContext(
            new DbContextOptionsBuilder<VendParametersContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "210100", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "210200", LedgerChartOfAccounts = 10 },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "210100", DisplayValue = "210100" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "210200", DisplayValue = "210200" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class VendParametersContext(DbContextOptions<VendParametersContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // VendTable is passed in as an object and never queried, so it stays unmapped: mapping
            // it drags in the DirPartyTable navigations, which this fixture has no need of.
            builder.Ignore<VendTable>();
            Type[] types = [typeof(VendParameters), typeof(VendLedgerAccounts),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination)];
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[])
                        && !property.PropertyType.IsValueType)
                        entity.Ignore(property.Name);
            }
            builder.Entity<VendParameters>().HasIndex(x => new { x.DataAreaId, x.Key }).IsUnique();
        }
    }
}
