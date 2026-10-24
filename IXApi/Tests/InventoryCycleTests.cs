using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class InventoryCycleTests
{
    // ---- Reservation engine (Phase 3 §7–8) --------------------------------------------------

    [Fact]
    public async Task Reserve_and_release_track_ownership_and_availability()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 100, ReservPhysical = 0, OnOrder = 30, AvailPhysical = 100 });
        db.Set<InventTransOrigin>().Add(new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "SO-L1", ItemId = "ITEM", ReferenceId = "SO1", ReferenceCategory = InventRefType.SalesTable });
        db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = -30, StatusIssue = StatusIssue.Ordered });
        await db.SaveChangesAsync();
        var service = new InventoryReservationService(new TestUnitOfWork(db), new TestCompany());

        // Reserve 20 of 30 demand against 100 on hand.
        var (reserved, error) = await service.ReserveAsync("SO-L1", 20m, false, default);
        Assert.Null(error);
        Assert.Equal(20m, reserved!.Reserved);
        Assert.Equal(80m, reserved.AvailablePhysical);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(20m, sum.ReservPhysical);
        Assert.Equal(10m, sum.OnOrder);
        Assert.Equal(80m, sum.AvailPhysical);
        Assert.Equal(20m, -(await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.ReservPhysical)).Qty);

        // Idempotent set-target: reserving the same 20 again is a no-op.
        var (same, _) = await service.ReserveAsync("SO-L1", 20m, false, default);
        Assert.Equal(20m, same!.Reserved);
        Assert.Equal(20m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        // Cannot reserve above remaining demand (30).
        Assert.NotNull((await service.ReserveAsync("SO-L1", 40m, false, default)).Error);

        // Release 5 → reserved 15, available 85.
        var (released, releaseError) = await service.ReleaseAsync("SO-L1", 5m, default);
        Assert.Null(releaseError);
        Assert.Equal(15m, released!.Reserved);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(15m, sum.ReservPhysical);
        Assert.Equal(15m, sum.OnOrder);
        Assert.Equal(85m, sum.AvailPhysical);
    }

    [Fact]
    public async Task Reserve_rejects_more_than_available_physical_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 10, ReservPhysical = 0, OnOrder = 30, AvailPhysical = 10 });
        db.Set<InventTransOrigin>().Add(new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "SO-L1", ItemId = "ITEM", ReferenceId = "SO1", ReferenceCategory = InventRefType.SalesTable });
        db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = -30, StatusIssue = StatusIssue.Ordered });
        await db.SaveChangesAsync();
        var service = new InventoryReservationService(new TestUnitOfWork(db), new TestCompany());

        Assert.NotNull((await service.ReserveAsync("SO-L1", 25m, false, default)).Error);
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        // allowPartial reserves only what is available (10).
        var (partial, error) = await service.ReserveAsync("SO-L1", 25m, true, default);
        Assert.Null(error);
        Assert.Equal(10m, partial!.Reserved);
        Assert.Equal(0m, partial.AvailablePhysical);
    }

    // ---- FIFO inventory close (Phase 7 worked example: 1,600 COGS / 600 remaining) -----------

    [Fact]
    public async Task Fifo_close_settles_issue_to_actual_fifo_cost_and_posts_adjustment()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        // Two financially posted receipts and one provisional-cost issue (running average 2200/200 = 11).
        db.Set<InventTransOrigin>().AddRange(
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "R1", ItemId = "ITEM", ReferenceId = "IJ1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 2, DataAreaId = "dat", InventTransId = "R2", ItemId = "ITEM", ReferenceId = "IJ2", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 3, DataAreaId = "dat", InventTransId = "I1", ItemId = "ITEM", ReferenceId = "IJ3", ReferenceCategory = InventRefType.InventoryJournal });
        db.Set<InventTrans>().AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 2, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1200, DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3, Qty = -150, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1650, DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 50, PostedValue = 550 });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(),
            new TestUser(), Sequence());
        var (result, error) = await service.CloseAsync(new DateTime(2026, 6, 30), default);

        Assert.Null(error);
        Assert.Equal(1, result!.IssuesSettled);
        Assert.Equal(2, result.SettlementsCreated);
        Assert.Equal(50m, result.TotalAdjustment);

        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(-1600m, issue.CostAmountPosted + issue.CostAmountAdjustment); // final FIFO COGS
        Assert.Equal(600m, (await db.Set<InventSum>().SingleAsync()).PostedValue); // remaining inventory value

        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(2, settlements.Count);
        Assert.Equal(-1600m, settlements.Sum(x => x.CostAmountSettled));
        Assert.Equal(-150m, settlements.Sum(x => x.QtySettled));

        var closing = await db.Set<InventClosing>().SingleAsync();
        Assert.Equal(InventCostStatus.Finished, closing.InventCostStatus);

        var ledgerLines = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(2, ledgerLines.Count);                              // one balanced Dr/Cr pair
        Assert.Equal(0m, ledgerLines.Sum(x => x.AccountingCurrencyAmount)); // debits and credits balance
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == 50m);  // Dr Inventory +50
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == -50m); // Cr COGS -50
    }

    [Fact]
    public async Task Fifo_close_is_idempotent_and_blocks_backdated_runs()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());

        var first = await service.CloseAsync(new DateTime(2026, 6, 30), default);
        Assert.Null(first.Error);
        var second = await service.CloseAsync(new DateTime(2026, 6, 30), default); // same cutoff -> idempotent
        Assert.Null(second.Error);
        Assert.Equal(first.Result!.RunNum, second.Result!.RunNum);
        Assert.Single(await db.Set<InventClosing>().ToListAsync());

        var backdated = await service.CloseAsync(new DateTime(2026, 5, 31), default); // before last close
        Assert.NotNull(backdated.Error);
    }

    private static void SeedLedgerAndPosting(CycleContext db)
    {
        db.Set<Ledger>().Add(new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", FiscalCalendar = 1 });
        db.Set<FiscalCalendarPeriod>().Add(new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) });
        db.Set<LedgerFiscalCalendarPeriod>().Add(new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open });
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "130100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "510100" });
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, DisplayValue = "130100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, DisplayValue = "510100" });
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 1, DataAreaId = "dat", InventAccountType = InventAccountType.InventReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 2, DataAreaId = "dat", InventAccountType = InventAccountType.InventIssue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
    }

    private static ISysNumberSequenceService Sequence() =>
        DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>();

    // ---- Harness ----------------------------------------------------------------------------

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public CycleContext Db { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture._connection.OpenAsync();
            fixture.Db = new CycleContext(new DbContextOptionsBuilder<CycleContext>().UseSqlite(fixture._connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            return fixture;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    private sealed class CycleContext(DbContextOptions<CycleContext> options) : DbContext(options)
    {
        private static readonly Type[] Included =
        [
            typeof(InventDim), typeof(InventTrans), typeof(InventTransOrigin), typeof(InventSum),
            typeof(InventModelGroup), typeof(InventModelGroupItem), typeof(InventItemGroupItem),
            typeof(InventPosting), typeof(DimensionAttributeValueCombination), typeof(MainAccount),
            typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
            typeof(InventSettlement), typeof(InventClosing), typeof(InventJournalTrans),
            typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry), typeof(SubledgerVoucherGeneralJournalEntry)
        ];

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var included = new HashSet<Type>(Included);
            foreach (var type in typeof(InventTrans).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity<long>).IsAssignableFrom(t) && !included.Contains(t)))
                builder.Ignore(type);
            foreach (var type in Included) Configure(builder, type);
        }

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

    public class TestSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name == nameof(ISysNumberSequenceService.NextAsync))
                return Task.FromResult(new NextSequenceResultDto { EntityName = "X", Value = ++_n, Code = $"X-{_n:D6}" });
            throw new NotSupportedException(method?.Name);
        }
    }

    private sealed class TestUser : ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        private IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) await _tx.CommitAsync(ct); }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) await _tx.RollbackAsync(ct); }
        public void Dispose() { }
    }
}
