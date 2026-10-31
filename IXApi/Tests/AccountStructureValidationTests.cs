using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using LedgerTable = IAX.IXApi.Modules.Finance.Entities.Ledger;

namespace IAX.IXApi.Tests;

/// <summary>
/// Posting-time validation of a ledger account combination against the account structures assigned
/// to the ledger.
/// </summary>
public class AccountStructureValidationTests
{
    /// <summary>
    /// The acceptance example from the core general ledger cycle, section 4 step 4, verbatim:
    /// "611000 ADMIN is valid. A blank Department on 611000 should fail under this proposed
    /// structure. A blank Department on 130100 can be valid."
    /// </summary>
    [Fact]
    public async Task The_cycle_docs_acceptance_example_holds()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SetUpStructuresAsync(db, assign: true);
        var validator = new AccountStructureValidator(db);

        // 611000 ADMIN is valid: a profit and loss account with a department.
        Assert.Null(await validator.ValidateAsync("dat",
            await CombinationAsync(db, "611000", ("Department", "ADMIN")), default));

        // A blank Department on 611000 fails: profit and loss requires one.
        var blankOnExpense = await validator.ValidateAsync("dat",
            await CombinationAsync(db, "611000", ("Department", "")), default);
        Assert.NotNull(blankOnExpense);
        Assert.Contains("requires a Department", blankOnExpense);
        Assert.Contains("611000", blankOnExpense);

        // A blank Department on 130100 is valid: the balance sheet structure makes it optional.
        Assert.Null(await validator.ValidateAsync("dat",
            await CombinationAsync(db, "130100", ("Department", "")), default));
    }

    [Fact]
    public async Task A_department_outside_the_structures_allowed_values_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SetUpStructuresAsync(db, assign: true);

        // LEGAL is a real dimension value, but the profit and loss structure allows only three.
        var refusal = await new AccountStructureValidator(db).ValidateAsync("dat",
            await CombinationAsync(db, "611000", ("Department", "LEGAL")), default);
        Assert.NotNull(refusal);
        Assert.Contains("'LEGAL' is not an allowed Department", refusal);
        Assert.Contains("'PL'", refusal);
    }

    // The main account value selects the structure, so an account in neither range has none.
    [Fact]
    public async Task A_main_account_no_structure_covers_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SetUpStructuresAsync(db, assign: true);

        var refusal = await new AccountStructureValidator(db).ValidateAsync("dat",
            await CombinationAsync(db, "950000", ("Department", "ADMIN")), default);
        Assert.Contains("not covered by any account structure assigned to the ledger", refusal);
    }

    // The user's decision: a company with no assigned structure posts as it did before.
    [Fact]
    public async Task A_ledger_with_no_assigned_structure_is_not_validated()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // Structures exist and are activated, but none is assigned to the ledger.
        await SetUpStructuresAsync(db, assign: false);

        // The combination that the profit and loss structure would refuse posts anyway.
        Assert.Null(await new AccountStructureValidator(db).ValidateAsync("dat",
            await CombinationAsync(db, "611000", ("Department", "")), default));
    }

    [Fact]
    public async Task A_draft_structure_assigned_to_nothing_constrains_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var builder = new AccountStructureBuilder(db);
        // Defined but never activated, so it has a tree yet is still a draft.
        await builder.DefineAsync("dat", ProfitAndLoss(), default);
        db.ChangeTracker.Clear();

        Assert.Null(await new AccountStructureValidator(db).ValidateAsync("dat",
            await CombinationAsync(db, "611000", ("Department", "")), default));
    }

    // String comparison, not numeric: 1000000 is seven digits but falls inside 100000..399999,
    // so it is validated by the balance sheet structure rather than falling through.
    [Fact]
    public async Task Structure_selection_compares_the_main_account_as_a_string()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SetUpStructuresAsync(db, assign: true);
        db.Add(new MainAccount { RecId = 99, DataAreaId = "dat", MainAccountId = "1000000", LedgerChartOfAccounts = 10 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // Balance sheet, so a blank department is fine — numerically it would be outside the range.
        Assert.Null(await new AccountStructureValidator(db).ValidateAsync("dat",
            await CombinationAsync(db, "1000000", ("Department", "")), default));
    }

    /// <summary>
    /// A combination written with only its own per-dimension column, and no value group chain, is
    /// still validated. Reading it as all blanks would let a bad department through.
    /// </summary>
    [Fact]
    public async Task A_combination_without_a_value_group_chain_is_validated_from_its_own_column()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SetUpStructuresAsync(db, assign: true);
        var account = await db.Set<MainAccount>().AsNoTracking().SingleAsync(x => x.MainAccountId == "611000");
        var combination = new DimensionAttributeValueCombination
        {
            DataAreaId = "dat", DisplayValue = "611000-LEGAL", MainAccount = account.RecId,
            MainAccountValue = "611000", DepartmentValue = "LEGAL"
        };
        db.Add(combination);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Contains("'LEGAL' is not an allowed Department",
            await new AccountStructureValidator(db).ValidateAsync("dat", combination, default));
    }

    private static async Task SetUpStructuresAsync(DbContext db, bool assign)
    {
        var builder = new AccountStructureBuilder(db);
        foreach (var definition in new[] { BalanceSheet(), ProfitAndLoss() })
        {
            await builder.DefineAsync("dat", definition, default);
            await builder.ActivateAsync("dat", definition.Name, default);
            if (assign) await builder.AssignToLedgerAsync("dat", definition.Name, default);
        }
        db.ChangeTracker.Clear();
    }

    private static async Task<DimensionAttributeValueCombination> CombinationAsync(
        DbContext db, string mainAccountId, params (string Dimension, string Value)[] values)
    {
        var account = await db.Set<MainAccount>().AsNoTracking().SingleAsync(x => x.MainAccountId == mainAccountId);
        var combination = await new LedgerDimensionResolver(db)
            .ResolveAsync("dat", account, values, accountStructure: 0, default);
        db.ChangeTracker.Clear();
        return combination;
    }

    private static AccountStructureCreateDto BalanceSheet() => new("BS", "Balance sheet accounts",
        [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "100000..399999"),
         new AccountStructureSegmentDto("Department", "*;\"\"")]);

    private static AccountStructureCreateDto ProfitAndLoss() => new("PL", "Profit and loss accounts",
        [new AccountStructureSegmentDto(AccountStructureBuilder.MainAccountSegment, "400000..899999"),
         new AccountStructureSegmentDto("Department", "ADMIN;SALES;SERVICE")]);

    private static async Task<ValidationContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new ValidationContext(new DbContextOptionsBuilder<ValidationContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new LedgerTable { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR" },
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "130100", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "611000", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "950000", LedgerChartOfAccounts = 10 },
            new DimensionAttribute { RecId = 1, DataAreaId = "dat", Name = "Department" },
            new DimensionAttributeValue { RecId = 1, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "ADMIN" },
            new DimensionAttributeValue { RecId = 2, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "SALES" },
            new DimensionAttributeValue { RecId = 3, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "SERVICE" },
            // A real dimension value that the profit and loss structure does not allow.
            new DimensionAttributeValue { RecId = 4, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "LEGAL" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class ValidationContext(DbContextOptions<ValidationContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(LedgerTable), typeof(MainAccount), typeof(DimensionAttribute),
                typeof(DimensionAttributeValue), typeof(DimensionAttributeValueCombination),
                typeof(DimensionAttributeValueGroup), typeof(DimensionAttributeLevelValue),
                typeof(DimensionAttributeValueGroupCombination), typeof(DimensionHierarchy),
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
