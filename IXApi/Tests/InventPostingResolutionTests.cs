using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Inventory posting profile resolution. An InventPosting rule is scoped on two axes: the item
/// (Table -> Group -> All) and the customer or vendor (Table -> Group -> All).
///
/// Resolution previously filtered the query to CustVendCode == All, so a rule configured for one
/// vendor or for a vendor group could never match. Nothing rejected such a rule at setup, so a
/// configured account was silently skipped and the posting landed on the All rule's account
/// instead. These tests pin the two-axis behavior.
/// </summary>
public class InventPostingResolutionTests
{
    private const string Area = "dat";

    [Fact]
    public async Task A_rule_scoped_to_one_vendor_wins_over_the_all_rule()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddRule(db, recId: 1, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.All, custVendRelation: "", dimension: 1);
        AddRule(db, recId: 2, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.Table, custVendRelation: "V-001", dimension: 2);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var support = new SubledgerPostingSupport(db);

        // With the vendor in context, the vendor-specific rule applies.
        var specific = await support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt,
            default, custVendAccount: "V-001");
        Assert.Equal(2L, specific.RecId);

        // A different vendor falls back to the All rule.
        var other = await support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt,
            default, custVendAccount: "V-999");
        Assert.Equal(1L, other.RecId);

        // A caller with no vendor in context - an inventory journal or the inventory close - must
        // not pick up a vendor-scoped rule.
        var none = await support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt, default);
        Assert.Equal(1L, none.RecId);
    }

    [Fact]
    public async Task A_vendor_group_rule_applies_and_a_vendor_rule_beats_it()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddRule(db, recId: 1, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.All, custVendRelation: "", dimension: 1);
        AddRule(db, recId: 2, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.Group, custVendRelation: "DOMESTIC", dimension: 2);
        AddRule(db, recId: 3, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.Table, custVendRelation: "V-001", dimension: 3);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var support = new SubledgerPostingSupport(db);

        // Group rule for a vendor in that group with no rule of its own.
        var byGroup = await support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt,
            default, custVendAccount: "V-002", custVendGroup: "DOMESTIC");
        Assert.Equal(2L, byGroup.RecId);

        // The vendor's own rule beats its group's.
        var byVendor = await support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt,
            default, custVendAccount: "V-001", custVendGroup: "DOMESTIC");
        Assert.Equal(3L, byVendor.RecId);
    }

    [Fact]
    public async Task The_item_axis_is_more_significant_than_the_customer_axis()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // One rule specific on the item only, one specific on the customer only. The item axis is
        // ranked first, matching how the inventory posting setup page is organised.
        AddRule(db, recId: 1, ItemCode: TableGroupAll.Table, itemRelation: "ITEM",
            custVendCode: TableGroupAll.All, custVendRelation: "", dimension: 1);
        AddRule(db, recId: 2, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.Table, custVendRelation: "C-001", dimension: 2);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var support = new SubledgerPostingSupport(db);
        var resolved = await support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt,
            default, custVendAccount: "C-001");
        Assert.Equal(1L, resolved.RecId);
    }

    [Fact]
    public async Task An_item_group_rule_still_resolves_through_the_item_axis()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = Area, ItemId = "ITEM", ItemGroupId = "TRADE" });
        AddRule(db, recId: 1, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.All, custVendRelation: "", dimension: 1);
        AddRule(db, recId: 2, ItemCode: TableGroupAll.Group, itemRelation: "TRADE",
            custVendCode: TableGroupAll.All, custVendRelation: "", dimension: 2);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var support = new SubledgerPostingSupport(db);
        // The item's group is looked up when the caller does not supply it.
        var resolved = await support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt, default);
        Assert.Equal(2L, resolved.RecId);
    }

    [Fact]
    public async Task Missing_setup_is_reported_rather_than_resolved_to_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // Only a rule for a different vendor exists, so nothing matches this posting.
        AddRule(db, recId: 1, ItemCode: TableGroupAll.All, itemRelation: "",
            custVendCode: TableGroupAll.Table, custVendRelation: "V-001", dimension: 1);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var support = new SubledgerPostingSupport(db);
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() =>
            support.ResolveInventPostingAsync(Area, "ITEM", InventAccountType.PurchReceipt,
                default, custVendAccount: "V-002"));
        Assert.Contains("Inventory posting setup is missing", error.Message);
    }

    private static void AddRule(PostingContext db, long recId, TableGroupAll ItemCode, string itemRelation,
        TableGroupAll custVendCode, string custVendRelation, long dimension) =>
        db.Add(new InventPosting
        {
            RecId = recId, DataAreaId = Area, InventAccountType = InventAccountType.PurchReceipt,
            ItemCode = ItemCode, ItemRelation = itemRelation,
            CustVendCode = custVendCode, CustVendRelation = custVendRelation,
            LedgerDimension = dimension
        });

    private static async Task<PostingContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PostingContext(new DbContextOptionsBuilder<PostingContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        // The account combinations the rules point at. RecId matches LedgerDimension so a resolved
        // rule is identifiable by the combination it returns.
        db.AddRange(
            new MainAccount { RecId = 1, DataAreaId = Area, MainAccountId = "140100", LedgerChartOfAccounts = 10 },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = Area, MainAccount = 1, MainAccountValue = "140100", DisplayValue = "140100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = Area, MainAccount = 1, MainAccountValue = "140100", DisplayValue = "140100" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = Area, MainAccount = 1, MainAccountValue = "140100", DisplayValue = "140100" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class PostingContext(DbContextOptions<PostingContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(InventPosting), typeof(InventItemGroupItem), typeof(MainAccount),
                typeof(DimensionAttributeValueCombination), typeof(LedgerStructure),
                typeof(DimensionHierarchy), typeof(Ledger)];
            var included = new HashSet<Type>(types);
            foreach (var type in typeof(InventPosting).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity<long>).IsAssignableFrom(t) && !included.Contains(t)))
                builder.Ignore(type);
            foreach (var type in types) Configure(builder, type);
        }

        /// <summary>Same shape the other inventory fixtures use: no row version, no navigations.</summary>
        private static void Configure(ModelBuilder builder, Type entity)
        {
            var e = builder.Entity(entity);
            e.HasKey(nameof(Entity<long>.RecId));
            e.Property(nameof(Entity<long>.RecId)).ValueGeneratedOnAdd();
            e.Ignore(nameof(Entity<long>.RowVersion));
            foreach (var property in entity.GetProperties())
            {
                var t = property.PropertyType;
                if (t != typeof(string) && t != typeof(byte[]) && !t.IsValueType)
                    e.Ignore(property.Name);
            }
        }
    }
}
