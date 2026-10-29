using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PostingRecord = IAX.IXApi.Modules.Finance.Entities.InventTransPosting;

namespace IAX.IXApi.Tests;

public class PackingSlipPhysicalPostingTests
{
    // Step 8.5 of the inventory cycle: the delivery posts the cost of units delivered, but only
    // when the item model group opts in to physical revenue posting.
    [Fact]
    public async Task Posts_the_delivered_cost_only_when_the_model_group_opts_in()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db, postPhysicalRevenue: NoYes.No);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var posting = new PackingSlipPhysicalPosting(db);
        Assert.False(await posting.PostAsync("dat", "ITEM-001", 1, "PS-000001", new DateTime(2026, 1, 10), 1500m, default));
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
        Assert.Empty(await db.Set<PostingRecord>().AsNoTracking().ToListAsync());

        // Opting in makes the same delivery post.
        var model = await db.Set<InventModelGroup>().SingleAsync();
        model.PostPhysicalRevenue = NoYes.Yes;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.True(await posting.PostAsync("dat", "ITEM-001", 1, "PS-000001", new DateTime(2026, 1, 10), 1500m, default));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var entry = await db.Set<GeneralJournalEntry>().AsNoTracking().SingleAsync();
        Assert.Equal("PS-000001", entry.SubledgerVoucher);
        Assert.Equal(new DateTime(2026, 1, 10), entry.AccountingDate);
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        // Inventory is credited and the cost of units delivered is debited.
        Assert.Equal(-1500m, lines.Single(x => x.PostingType == LedgerPostingType.SalesPackingSlip).AccountingCurrencyAmount);
        Assert.Equal(1500m, lines.Single(x => x.PostingType == LedgerPostingType.SalesOffsetAccountPackingSlip).AccountingCurrencyAmount);

        var record = await db.Set<PostingRecord>().AsNoTracking().SingleAsync();
        Assert.Equal(InventTransPostingType.Physical, record.InventTransPostingType);
        Assert.Equal(NoYes.Yes, record.IsPosted);
        Assert.Equal("PS-000001", record.Voucher);
        Assert.Equal(1, record.InventTransOrigin);
        Assert.Equal(10, record.LedgerDimension);
        Assert.Equal(20, record.OffsetLedgerDimension);

        // A zero cost posts nothing: there is no economic effect to record.
        Assert.False(await posting.PostAsync("dat", "ITEM-001", 1, "PS-000002", new DateTime(2026, 1, 10), 0m, default));
    }

    // The invoice takes over from the delivery, so the delivery's physical posting is reversed
    // onto the invoice voucher using the accounts the packing slip recorded.
    [Fact]
    public async Task Invoice_reverses_the_delivery_posting_once_using_its_original_accounts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db, postPhysicalRevenue: NoYes.Yes);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var posting = new PackingSlipPhysicalPosting(db);
        Assert.True(await posting.PostAsync("dat", "ITEM-001", 1, "PS-000001", new DateTime(2026, 1, 10), 1500m, default));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // The invoice's own posted entry carries the reversal.
        db.Add(new GeneralJournalEntry { RecId = 900, DataAreaId = "dat", Ledger = 1, SubledgerVoucher = "INV-000001", AccountingDate = new DateTime(2026, 1, 20) });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await posting.ReverseAsync("dat", 1, 900, new DateTime(2026, 1, 20), "SAR", 1500m, default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var reversal = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.GeneralJournalEntry == 900).ToListAsync();
        Assert.Equal(2, reversal.Count);
        Assert.Equal(0m, reversal.Sum(x => x.AccountingCurrencyAmount));
        // The original credit to inventory comes back as a debit, on the same account.
        Assert.Equal(1500m, reversal.Single(x => x.LedgerDimension == 10).AccountingCurrencyAmount);
        Assert.Equal(-1500m, reversal.Single(x => x.LedgerDimension == 20).AccountingCurrencyAmount);
        // Delivery and invoice together leave no physical posting in the ledger.
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync())
            .Where(x => x.PostingType is LedgerPostingType.SalesPackingSlip or LedgerPostingType.SalesOffsetAccountPackingSlip)
            .Sum(x => x.AccountingCurrencyAmount));

        // The record is no longer in force, so a second invoice cannot reverse it again.
        Assert.Equal(NoYes.No, (await db.Set<PostingRecord>().AsNoTracking().SingleAsync()).IsPosted);
        await posting.ReverseAsync("dat", 1, 900, new DateTime(2026, 1, 20), "SAR", 1500m, default);
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.Set<GeneralJournalAccountEntry>().AsNoTracking().CountAsync(x => x.GeneralJournalEntry == 900));
    }

    [Fact]
    public async Task Reversal_does_nothing_when_the_delivery_posted_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db, postPhysicalRevenue: NoYes.No);
        db.Add(new GeneralJournalEntry { RecId = 900, DataAreaId = "dat", Ledger = 1, SubledgerVoucher = "INV-000001", AccountingDate = new DateTime(2026, 1, 20) });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new PackingSlipPhysicalPosting(db).ReverseAsync("dat", 1, 900, new DateTime(2026, 1, 20), "SAR", 1500m, default);
        await db.SaveChangesAsync();
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
    }

    private static void Seed(DbContext db, NoYes postPhysicalRevenue)
    {
        db.AddRange(
            new Ledger { RecId = 1, DataAreaId = "dat", FiscalCalendar = 1, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) },
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "ORG-000001", ItemId = "ITEM-001" },
            new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", PostOnHandPhysical = NoYes.Yes,
                PostPhysicalRevenue = postPhysicalRevenue },
            new InventModelGroupItem { RecId = 1, ItemId = "ITEM-001", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "140100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "510100" },
            new DimensionAttributeValueCombination { RecId = 10, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "140100", DisplayValue = "140100" },
            new DimensionAttributeValueCombination { RecId = 20, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "510100", DisplayValue = "510100" },
            new InventPosting { RecId = 1, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlip,
                ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 10 },
            new InventPosting { RecId = 2, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount,
                ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 20 });
    }

    private static async Task<SlipContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new SlipContext(new DbContextOptionsBuilder<SlipContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class SlipContext(DbContextOptions<SlipContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(InventTransOrigin), typeof(InventModelGroup), typeof(InventModelGroupItem),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination), typeof(InventPosting),
                typeof(InventItemGroupItem), typeof(PostingRecord),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry), typeof(SubledgerVoucherGeneralJournalEntry)];
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[]) && !property.PropertyType.IsValueType)
                        entity.Ignore(property.Name);
            }
        }
    }
}
