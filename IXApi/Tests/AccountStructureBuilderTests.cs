using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using LedgerTable = IAX.IXApi.Modules.Finance.Entities.Ledger;

namespace IAX.IXApi.Tests;

/// <summary>
/// Account structures as the core general ledger cycle defines them: a balance sheet structure over
/// 100000..399999 with Department optional, and a profit and loss structure over 400000..899999
/// with Department mandatory and limited to three values.
/// </summary>
public class AccountStructureBuilderTests
{
    [Fact]
    public async Task Defining_a_structure_writes_its_segments_and_constraint_tree_as_a_draft()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var (structure, error) = await new AccountStructureBuilder(db).DefineAsync("dat", BalanceSheet(), default);
        Assert.Null(error);
        Assert.NotNull(structure);
        Assert.Equal(NoYes.Yes, structure!.IsDraft);
        Assert.Equal(DimensionHierarchyStructureType.AccountStructure, structure.StructureType);

        db.ChangeTracker.Clear();
        var levels = await db.Set<DimensionHierarchyLevel>().AsNoTracking().OrderBy(x => x.Level).ToListAsync();
        Assert.Equal(2, levels.Count);
        Assert.Equal(0, levels[0].DimensionAttribute); // The main account segment carries no dimension.
        Assert.NotEqual(0, levels[1].DimensionAttribute);

        var nodes = await db.Set<DimensionConstraintNode>().AsNoTracking().ToListAsync();
        Assert.Equal(2, nodes.Count);
        var mainAccount = nodes.Single(x => x.Name == AccountStructureBuilder.MainAccountSegment);
        var department = nodes.Single(x => x.Name == "Department");
        // The tree is a chain: the department node hangs off the main account node.
        Assert.Equal(0, mainAccount.ParentConstraintNode);
        Assert.Equal(mainAccount.RecId, department.ParentConstraintNode);
        // Department is optional on the balance sheet, because its criteria include "".
        Assert.Equal(NoYes.No, mainAccount.IsOptional);
        Assert.Equal(NoYes.Yes, department.IsOptional);

        var criteria = await db.Set<DimensionConstraintNodeCriteria>().AsNoTracking().ToListAsync();
        var range = criteria.Single(x => x.DimensionConstraintNode == mainAccount.RecId);
        Assert.Equal("100000", range.RangeFrom);
        Assert.Equal("399999", range.RangeTo);
        // The "" that makes the segment optional is the IsOptional flag, not a criterion row.
        Assert.Equal("*", criteria.Single(x => x.DimensionConstraintNode == department.RecId).WildCardString);
    }

    [Fact]
    public async Task Activating_clears_the_draft_flag_and_assignment_needs_an_activated_structure()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var builder = new AccountStructureBuilder(db);
        await builder.DefineAsync("dat", BalanceSheet(), default);

        // A draft cannot be assigned to the ledger.
        var (blocked, blockedError) = await builder.AssignToLedgerAsync("dat", "BS", default);
        Assert.Null(blocked);
        Assert.Contains("Activate 'BS' before assigning", blockedError);

        var (activated, error) = await builder.ActivateAsync("dat", "BS", default);
        Assert.Null(error);
        Assert.Equal(NoYes.No, activated!.IsDraft);

        var (assigned, assignError) = await builder.AssignToLedgerAsync("dat", "BS", default);
        Assert.Null(assignError);
        Assert.NotNull(assigned);
        db.ChangeTracker.Clear();
        var link = await db.Set<LedgerStructure>().AsNoTracking().SingleAsync();
        Assert.Equal(activated.RecId, link.DimensionHierarchy);

        // Assigning twice does not duplicate the link.
        await builder.AssignToLedgerAsync("dat", "BS", default);
        db.ChangeTracker.Clear();
        Assert.Single(await db.Set<LedgerStructure>().AsNoTracking().ToListAsync());
    }

    // The cycle doc requires the two structures' main account ranges to be non-overlapping.
    [Fact]
    public async Task The_cycle_docs_balance_sheet_and_profit_and_loss_structures_both_activate()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var builder = new AccountStructureBuilder(db);

        await builder.DefineAsync("dat", BalanceSheet(), default);
        Assert.Null((await builder.ActivateAsync("dat", "BS", default)).Error);
        await builder.DefineAsync("dat", ProfitAndLoss(), default);
        Assert.Null((await builder.ActivateAsync("dat", "PL", default)).Error);

        db.ChangeTracker.Clear();
        var described = await builder.DescribeAsync("dat",
            await db.Set<DimensionHierarchy>().AsNoTracking().SingleAsync(x => x.Name == "PL"), default);
        Assert.False(described.IsDraft);
        var department = described.Segments.Single(x => x.Dimension == "Department");
        // Mandatory on profit and loss: no "" criterion, so blank is not allowed.
        Assert.False(department.AllowsBlank);
        Assert.Equal("ADMIN;SALES;SERVICE", department.AllowedValues);
    }

    // The main account value selects the structure, so it may belong to only one active structure.
    [Fact]
    public async Task Activating_a_structure_whose_main_accounts_overlap_an_active_one_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var builder = new AccountStructureBuilder(db);
        await builder.DefineAsync("dat", BalanceSheet(), default);
        await builder.ActivateAsync("dat", "BS", default);

        await builder.DefineAsync("dat", new AccountStructureCreateDto("BS2", "Overlapping",
            [
                new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "200000..250000"),
                new AccountStructureSegmentDto("Department", "*;\"\"")
            ]), default);
        var (result, error) = await builder.ActivateAsync("dat", "BS2", default);

        Assert.Null(result);
        Assert.Contains("overlap active account structure 'BS'", error);
        db.ChangeTracker.Clear();
        // The overlapping structure stays a draft, so it never constrains anything.
        Assert.Equal(NoYes.Yes,
            (await db.Set<DimensionHierarchy>().AsNoTracking().SingleAsync(x => x.Name == "BS2")).IsDraft);
    }

    [Theory]
    [InlineData("", "A structure with no main account segment", "must include a 'MainAccount' segment")]
    [InlineData("Region", "An unknown dimension", "Financial dimension 'Region' does not exist")]
    public async Task A_structure_that_cannot_be_built_is_refused(string dimension, string _, string expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var segments = dimension.Length == 0
            ? new List<AccountStructureSegmentDto> { new("Department", "*") }
            : [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "100000..399999"),
               new AccountStructureSegmentDto(dimension, "*")];
        var (result, error) = await new AccountStructureBuilder(db)
            .DefineAsync("dat", new AccountStructureCreateDto("X", null, segments), default);

        Assert.Null(result);
        Assert.Contains(expected, error);
        db.ChangeTracker.Clear();
        // Nothing partial is left behind.
        Assert.Empty(await db.Set<DimensionHierarchy>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task A_duplicate_name_and_a_repeated_dimension_are_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var builder = new AccountStructureBuilder(db);
        await builder.DefineAsync("dat", BalanceSheet(), default);

        Assert.Contains("already exists", (await builder.DefineAsync("dat", BalanceSheet(), default)).Error);
        Assert.Contains("may appear only once", (await builder.DefineAsync("dat",
            new AccountStructureCreateDto("Twice", null,
                [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "1..2"),
                 new AccountStructureSegmentDto("Department", "*"),
                 new AccountStructureSegmentDto("Department", "*")]), default)).Error);
    }

    // A segment allowing only blank could never carry a value, which is a setup mistake.
    [Fact]
    public async Task A_segment_allowing_only_blank_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var (result, error) = await new AccountStructureBuilder(db).DefineAsync("dat",
            new AccountStructureCreateDto("OnlyBlank", null,
                [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "100000..399999"),
                 new AccountStructureSegmentDto("Department", "\"\"")]), default);

        Assert.Null(result);
        Assert.Contains("allows only blank", error);
    }

    private static AccountStructureCreateDto BalanceSheet() => new("BS", "Balance sheet accounts",
        [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "100000..399999"),
         new AccountStructureSegmentDto("Department", "*;\"\"")]);

    private static AccountStructureCreateDto ProfitAndLoss() => new("PL", "Profit and loss accounts",
        [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "400000..899999"),
         new AccountStructureSegmentDto("Department", "ADMIN;SALES;SERVICE")]);

    private static async Task<StructureContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new StructureContext(new DbContextOptionsBuilder<StructureContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new LedgerTable { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR" },
            new DimensionAttribute { RecId = 1, DataAreaId = "dat", Name = "Department" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class StructureContext(DbContextOptions<StructureContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(LedgerTable), typeof(DimensionAttribute), typeof(DimensionHierarchy),
                typeof(DimensionHierarchyLevel), typeof(DimensionConstraintTree),
                typeof(DimensionConstraintNode), typeof(DimensionConstraintNodeCriteria),
                typeof(LedgerStructure)];
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
            }
        }
    }
}
