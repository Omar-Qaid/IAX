using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTransPosting;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PostingRecord = IAX.IXApi.Modules.Finance.Entities.InventTransPosting;

namespace IAX.IXApi.Tests;

public class InventTransPostingTests
{
    // The posting record is the bridge from an inventory transaction to the voucher that posted it.
    [Fact]
    public void Record_captures_both_accounts_their_posting_types_and_the_voucher()
    {
        var inventory = new DimensionAttributeValueCombination { RecId = 11, DataAreaId = "dat", DisplayValue = "140100" };
        var offset = new DimensionAttributeValueCombination { RecId = 22, DataAreaId = "dat", DisplayValue = "210100" };

        var record = InventTransPostingService.Record("dat", 500, "ITEM-001",
            InventTransPostingType.Physical, new DateTime(2026, 1, 10), "VOU-000001",
            inventory, LedgerPostingType.PurchReceipt, offset, LedgerPostingType.LedgerJournal);

        Assert.Equal("dat", record.DataAreaId);
        Assert.Equal(500, record.InventTransOrigin);
        Assert.Equal("ITEM-001", record.ItemId);
        Assert.Equal(InventTransPostingType.Physical, record.InventTransPostingType);
        Assert.Equal(NoYes.Yes, record.IsPosted);
        Assert.Equal(new DateTime(2026, 1, 10), record.TransDate);
        Assert.Equal("VOU-000001", record.Voucher);
        Assert.Equal(11, record.LedgerDimension);
        Assert.Equal(LedgerPostingType.PurchReceipt, record.PostingType);
        Assert.Equal(22, record.OffsetLedgerDimension);
        Assert.Equal(LedgerPostingType.LedgerJournal, record.PostingTypeOffset);
        Assert.Equal(string.Empty, record.ProjId);
    }

    [Fact]
    public async Task Reads_posting_records_by_inventory_transaction_and_by_voucher()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.AddRange(
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "ORG-000001", ItemId = "ITEM-001" },
            new InventTransOrigin { RecId = 2, DataAreaId = "dat", InventTransId = "ORG-000002", ItemId = "ITEM-002" },
            // Two records for one origin: the physical receipt, then its financial update.
            Posting(1, 1, "ITEM-001", InventTransPostingType.Physical, "VOU-000001", new DateTime(2026, 1, 10)),
            Posting(2, 1, "ITEM-001", InventTransPostingType.Financial, "VOU-000002", new DateTime(2026, 1, 20)),
            Posting(3, 2, "ITEM-002", InventTransPostingType.Financial, "VOU-000002", new DateTime(2026, 1, 20)),
            // Another company's record must never appear.
            Posting(4, 1, "ITEM-001", InventTransPostingType.Financial, "VOU-000002", new DateTime(2026, 1, 20), area: "other"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = new InventTransPostingService(new TestUnitOfWork(db), new TestUser());

        var (byTrans, transError) = await service.ByInventTransIdAsync("ORG-000001", default);
        Assert.Null(transError);
        Assert.Equal(2, byTrans!.Count);
        // Ordered by accounting date: physical first, then financial.
        Assert.Equal(InventTransPostingType.Physical, byTrans[0].InventTransPostingType);
        Assert.Equal(InventTransPostingType.Financial, byTrans[1].InventTransPostingType);
        Assert.All(byTrans, x => Assert.Equal("ORG-000001", x.InventTransId));

        var (byVoucher, voucherError) = await service.ByVoucherAsync("VOU-000002", default);
        Assert.Null(voucherError);
        Assert.Equal(2, byVoucher!.Count);
        Assert.Equal(["ITEM-001", "ITEM-002"], byVoucher.Select(x => x.ItemId).Order());

        Assert.Empty((await service.ByInventTransIdAsync("ORG-000099", default)).Items!);
        Assert.Contains("identifier is required", (await service.ByInventTransIdAsync(" ", default)).Error);
        Assert.Contains("voucher is required", (await service.ByVoucherAsync("", default)).Error);
    }

    private static PostingRecord Posting(long recId, long origin, string itemId,
        InventTransPostingType type, string voucher, DateTime date, string area = "dat") => new()
        {
            RecId = recId, DataAreaId = area, InventTransOrigin = origin, ItemId = itemId,
            InventTransPostingType = type, IsPosted = NoYes.Yes, Voucher = voucher,
            TransDate = date, TransBeginTime = date, LedgerDimension = 11, OffsetLedgerDimension = 22,
            PostingType = LedgerPostingType.InventReceipt, PostingTypeOffset = LedgerPostingType.LedgerJournal,
            ProjId = string.Empty
        };

    private static async Task<PostingContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PostingContext(new DbContextOptionsBuilder<PostingContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class PostingContext(DbContextOptions<PostingContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(PostingRecord), typeof(InventTransOrigin)];
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

    private sealed class TestUser : ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }
}
