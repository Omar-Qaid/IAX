using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.Settlement;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public class LedgerSettlementTests
{
    // Cases J and K of the cycle: a receipt is parked in clearing, then identified. Settling the
    // two clearing entries changes matching data only; the account balance is untouched.
    [Fact]
    public async Task Settles_equal_same_currency_entries_and_removes_them_from_open_items()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = NewService(db);
        var balanceBefore = await ClearingBalanceAsync(db);
        var (open, _) = await service.OpenItemsAsync("250100", default);
        // Four entries sit on the clearing account: the SAR pair and a second pair held in USD.
        Assert.Equal(4, open!.Count);

        var (result, error) = await service.SettleAsync(new([1, 2], new DateTime(2026, 1, 22)), default);

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(2, result!.EntriesSettled);
        Assert.Equal(3000m, result.SettledAmount);
        Assert.Equal(new DateTime(2026, 1, 22), result.SettleDate);
        Assert.StartsWith("LSET", result.SettleId);

        db.ChangeTracker.Clear();
        var rows = await db.Set<LedgerTransSettlement>().AsNoTracking().OrderBy(x => x.TransRecId).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, x => Assert.Equal(result.SettleId, x.SettleId));
        Assert.All(rows, x => Assert.Equal(new DateTime(2026, 1, 22), x.SettleDate));
        Assert.All(rows, x => Assert.Equal(NoYes.No, x.IsReversed));
        Assert.Equal(-3000m, rows[0].SettleAccountingCurrencyAmount);
        Assert.Equal(3000m, rows[1].SettleAccountingCurrencyAmount);
        // No value is invented for the fields D365 leaves empty here.
        Assert.All(rows, x => Assert.Equal(0, x.SettlementTableId));
        Assert.All(rows, x => Assert.Equal(0L, x.SettlementGroup));
        Assert.All(rows, x => Assert.Equal(0L, x.RefTransRecId));

        // Settlement is matching, not accounting: no voucher is posted and the balance is unchanged.
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().Where(x => x.RecId > 2).ToListAsync());
        Assert.Equal(balanceBefore, await ClearingBalanceAsync(db));

        var (afterSettle, _) = await service.OpenItemsAsync("250100", default);
        // Only the settled pair leaves the open items; the unsettled USD pair stays.
        Assert.Equal([4L, 5L], afterSettle!.Select(x => x.AccountEntryRecId));
        // The revenue entries on another account are untouched by this settlement.
        var (revenue, _) = await service.OpenItemsAsync("410100", default);
        Assert.Single(revenue!);
    }

    [Fact]
    public async Task Reversing_a_settlement_returns_its_entries_to_open_items()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = NewService(db);
        var (settled, _) = await service.SettleAsync(new([1, 2], null), default);
        db.ChangeTracker.Clear();

        var (reversed, error) = await service.ReverseAsync(settled!.SettleId, default);
        Assert.Null(error);
        Assert.Equal(2, reversed!.EntriesSettled);

        db.ChangeTracker.Clear();
        var rows = await db.Set<LedgerTransSettlement>().AsNoTracking().ToListAsync();
        // The history stays; the match no longer stands.
        Assert.Equal(2, rows.Count);
        Assert.All(rows, x => Assert.Equal(NoYes.Yes, x.IsReversed));
        Assert.All(rows, x => Assert.Equal(settled.SettleId, x.OriginalSettleId));

        var (open, _) = await service.OpenItemsAsync("250100", default);
        Assert.Equal(4, open!.Count);

        // The same entries can be settled again once the earlier match is reversed.
        var (again, againError) = await service.SettleAsync(new([1, 2], null), default);
        Assert.Null(againError);
        Assert.NotEqual(settled.SettleId, again!.SettleId);

        var (_, twice) = await service.ReverseAsync(settled.SettleId, default);
        Assert.Contains("already reversed", twice);
        var (missing, noError) = await service.ReverseAsync("LSET-999999", default);
        Assert.Null(missing);
        Assert.Null(noError);
    }

    [Fact]
    public async Task Refuses_an_unbalanced_selection_a_second_settlement_and_a_mixed_account()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = NewService(db);
        Assert.Contains("at least two", (await service.SettleAsync(new([1], null), default)).Error);
        // Entry 3 is revenue, not clearing.
        Assert.Contains("one main account", (await service.SettleAsync(new([1, 3], null), default)).Error);
        Assert.Contains("does not exist", (await service.SettleAsync(new([1, 404], null), default)).Error);

        // An out-of-balance selection would need write-off or exchange accounting, which this path refuses.
        var partial = await db.Set<GeneralJournalAccountEntry>().SingleAsync(x => x.RecId == 2);
        partial.AccountingCurrencyAmount = 2800m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Contains("out of balance", (await service.SettleAsync(new([1, 2], null), default)).Error);
        Assert.Empty(await db.Set<LedgerTransSettlement>().AsNoTracking().ToListAsync());

        partial = await db.Set<GeneralJournalAccountEntry>().SingleAsync(x => x.RecId == 2);
        partial.AccountingCurrencyAmount = 3000m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var (first, _) = await service.SettleAsync(new([1, 2], null), default);
        Assert.NotNull(first);
        db.ChangeTracker.Clear();
        Assert.Contains("already settled", (await service.SettleAsync(new([1, 2], null), default)).Error);

        var (mismatch, currencyError) = await service.SettleAsync(new([4, 5], null), default);
        Assert.Null(mismatch);
        Assert.Contains("one transaction currency", currencyError);

        Assert.Contains("was not found", (await service.OpenItemsAsync("999999", default)).Error);
    }

    private static async Task<decimal> ClearingBalanceAsync(DbContext db) =>
        await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.MainAccount == 1).SumAsync(x => x.AccountingCurrencyAmount);

    private static void Seed(DbContext db)
    {
        db.AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "250100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "410100" },
            new GeneralJournalEntry { RecId = 1, DataAreaId = "dat", SubledgerVoucher = "V1", AccountingDate = new DateTime(2026, 1, 20) },
            new GeneralJournalEntry { RecId = 2, DataAreaId = "dat", SubledgerVoucher = "V2", AccountingDate = new DateTime(2026, 1, 22) },
            // Case J: unidentified receipt credits clearing. Case K: identification debits it back.
            Entry(1, entry: 1, account: 1, amount: -3000m, text: "Unidentified receipt"),
            Entry(2, entry: 2, account: 1, amount: 3000m, text: "Receipt identified"),
            Entry(3, entry: 2, account: 2, amount: -3000m, text: "Service revenue"),
            // A pair on the clearing account held in another transaction currency.
            Entry(4, entry: 1, account: 1, amount: -100m, text: "USD receipt", currency: "USD"),
            Entry(5, entry: 2, account: 1, amount: 100m, text: "SAR offset"));
    }

    private static GeneralJournalAccountEntry Entry(long recId, long entry, long account, decimal amount,
        string text, string currency = "SAR") => new()
        {
            RecId = recId, DataAreaId = "dat", GeneralJournalEntry = entry, MainAccount = account,
            LedgerDimension = account, LedgerAccount = account == 1 ? "250100" : "410100",
            PostingType = LedgerPostingType.LedgerJournal, TransactionCurrencyCode = currency,
            TransactionCurrencyAmount = amount, AccountingCurrencyAmount = amount, ReportingCurrencyAmount = amount,
            IsCredit = amount < 0 ? NoYes.Yes : NoYes.No, Text = text, PaymentReference = string.Empty
        };

    private static LedgerSettlementService NewService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), DispatchProxy.Create<ISysNumberSequenceService, SettlementSequenceProxy>());

    private static async Task<SettlementContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new SettlementContext(new DbContextOptionsBuilder<SettlementContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class SettlementContext(DbContextOptions<SettlementContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(MainAccount), typeof(GeneralJournalEntry),
                typeof(GeneralJournalAccountEntry), typeof(LedgerTransSettlement),
                typeof(IAX.IXApi.Modules.Finance.Entities.LedgerParameters)];
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

    public class SettlementSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            method?.Name == nameof(ISysNumberSequenceService.NextAsync)
                ? Task.FromResult(new NextSequenceResultDto { EntityName = "LedgerTransSettlement", Value = ++_n, Code = $"LSET-{_n:D6}" })
                : throw new NotSupportedException(method?.Name);
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
