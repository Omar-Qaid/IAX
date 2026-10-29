using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public class LedgerTrialBalanceTests
{
    // Section 8 of the cycle: the January trial balance after Cases A through N.
    // Totals are 131,000 on each side and profit is 13,900.
    [Fact]
    public async Task Reproduces_the_january_trial_balance_of_the_core_cycle()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewService(db).GetAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), CurrentToOperationsPostingLayer.Current, default);

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.True(result!.Balanced);
        Assert.Equal("SAR", result.AccountingCurrency);
        Assert.Equal(131000m, result.TotalDebitBalance);
        Assert.Equal(131000m, result.TotalCreditBalance);

        var byAccount = result.Lines.ToDictionary(x => x.MainAccountId);
        Assert.Equal(108900m, byAccount["110100"].DebitBalance);
        Assert.Equal(11000m, byAccount["130100"].DebitBalance);
        Assert.Equal(2000m, byAccount["220100"].CreditBalance);
        Assert.Equal(4000m, byAccount["230100"].CreditBalance);
        Assert.Equal(100000m, byAccount["310100"].CreditBalance);
        Assert.Equal(25000m, byAccount["410100"].CreditBalance);
        Assert.Equal(1000m, byAccount["611000"].DebitBalance);
        Assert.Equal(8000m, byAccount["612000"].DebitBalance);
        Assert.Equal(2000m, byAccount["613000"].DebitBalance);
        Assert.Equal(100m, byAccount["614000"].DebitBalance);
        Assert.Equal("Bank ledger control", byAccount["110100"].Name);

        // Accounts that settle to zero keep their gross movements and show no balance.
        Assert.Equal(8000m, byAccount["220200"].Debit);
        Assert.Equal(8000m, byAccount["220200"].Credit);
        Assert.Equal(0m, byAccount["220200"].DebitBalance);
        Assert.Equal(0m, byAccount["220200"].CreditBalance);
        Assert.Equal(0m, byAccount["250100"].DebitBalance);

        // Profit is revenue less the expense accounts.
        var profit = byAccount["410100"].CreditBalance
            - new[] { "611000", "612000", "613000", "614000" }.Sum(x => byAccount[x].DebitBalance);
        Assert.Equal(13900m, profit);

        // Accounts are listed in account order.
        Assert.Equal(result.Lines.Select(x => x.MainAccountId).OrderBy(x => x, StringComparer.Ordinal),
            result.Lines.Select(x => x.MainAccountId));
    }

    [Fact]
    public async Task Scopes_by_date_range_posting_layer_and_ledger()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        // February activity, an adjustment-layer voucher, and another ledger's voucher.
        db.AddRange(
            Entry(900, new DateTime(2026, 2, 1)),
            Entry(901, new DateTime(2026, 1, 15), layer: CurrentToOperationsPostingLayer.Operations),
            Entry(902, new DateTime(2026, 1, 15), ledger: 2));
        db.AddRange(
            Line(900, 900, 11, 500m), Line(901, 900, 1, -500m),
            Line(902, 901, 11, 700m), Line(903, 901, 1, -700m),
            Line(904, 902, 11, 900m), Line(905, 902, 1, -900m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = NewService(db);
        var (january, _) = await service.GetAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31),
            CurrentToOperationsPostingLayer.Current, default);
        // None of the three extra vouchers belongs to January/Current/this ledger.
        Assert.Equal(131000m, january!.TotalDebitBalance);

        var (february, _) = await service.GetAsync(new DateTime(2026, 2, 1), new DateTime(2026, 2, 28),
            CurrentToOperationsPostingLayer.Current, default);
        Assert.Equal(500m, february!.TotalDebitBalance);
        Assert.True(february.Balanced);

        var (adjustment, _) = await service.GetAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31),
            CurrentToOperationsPostingLayer.Operations, default);
        Assert.Equal(700m, adjustment!.TotalDebitBalance);

        Assert.Contains("cannot precede", (await service.GetAsync(
            new DateTime(2026, 1, 31), new DateTime(2026, 1, 1), CurrentToOperationsPostingLayer.Current, default)).Error);
    }

    private static void Seed(DbContext db)
    {
        var accounts = new (long RecId, string Id, string Name)[]
        {
            (1, "110100", "Bank ledger control"), (2, "130100", "Prepaid rent"),
            (3, "220100", "Accrued utilities"), (4, "220200", "Salaries payable"),
            (5, "230100", "Deferred revenue"), (6, "250100", "Receipt clearing"),
            (7, "310100", "Share capital"), (8, "410100", "Service revenue"),
            (9, "611000", "Rent expense"), (10, "612000", "Salaries expense"),
            (11, "613000", "Utilities expense"), (12, "614000", "Bank charges")
        };
        foreach (var (recId, id, name) in accounts)
            db.Add(new MainAccount { RecId = recId, DataAreaId = "dat", MainAccountId = id, Name = name });
        db.Add(new IAX.IXApi.Modules.Finance.Entities.Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR" });

        // Cases A..N as posted vouchers; the allocation and reclassification net to zero per account.
        var cases = new (long Entry, DateTime Date, (long Account, decimal Amount)[] Lines)[]
        {
            (1, new DateTime(2026, 1, 1), [(1, 100000m), (7, -100000m)]),      // A owner funding
            (2, new DateTime(2026, 1, 5), [(1, 20000m), (8, -20000m)]),        // B cash revenue
            (3, new DateTime(2026, 1, 6), [(2, 12000m), (1, -12000m)]),        // C prepaid rent
            (4, new DateTime(2026, 1, 31), [(9, 1000m), (2, -1000m)]),         // D recognize rent
            (5, new DateTime(2026, 1, 31), [(11, 2000m), (3, -2000m)]),        // E accrue utilities
            (6, new DateTime(2026, 1, 25), [(10, 8000m), (4, -8000m)]),        // F salary accrual
            (7, new DateTime(2026, 1, 28), [(4, 8000m), (1, -8000m)]),         // G pay salaries
            (8, new DateTime(2026, 1, 15), [(1, 6000m), (5, -6000m)]),         // H advance income
            (9, new DateTime(2026, 1, 31), [(5, 2000m), (8, -2000m)]),         // I earn one month
            (10, new DateTime(2026, 1, 20), [(1, 3000m), (6, -3000m)]),        // J unidentified cash
            (11, new DateTime(2026, 1, 22), [(6, 3000m), (8, -3000m)]),        // K identify the cash
            (12, new DateTime(2026, 1, 31), [(12, 100m), (1, -100m)]),         // L bank fee
            (13, new DateTime(2026, 1, 31), [(9, 600m), (9, 400m), (9, -1000m)]),   // M allocate rent
            (14, new DateTime(2026, 1, 31), [(11, 2000m), (11, -2000m)])            // N reclassify utilities
        };
        var lineId = 1L;
        foreach (var (entry, date, lines) in cases)
        {
            db.Add(Entry(entry, date));
            foreach (var (account, amount) in lines) db.Add(Line(lineId++, entry, account, amount));
        }
    }

    private static GeneralJournalEntry Entry(long recId, DateTime date,
        CurrentToOperationsPostingLayer layer = CurrentToOperationsPostingLayer.Current, long ledger = 1) => new()
        {
            RecId = recId, DataAreaId = "dat", Ledger = ledger, PostingLayer = layer,
            AccountingDate = date, DocumentDate = date, SubledgerVoucher = $"V{recId}", JournalNumber = "J1"
        };

    private static GeneralJournalAccountEntry Line(long recId, long entry, long account, decimal amount) => new()
    {
        RecId = recId, DataAreaId = "dat", GeneralJournalEntry = entry, MainAccount = account,
        LedgerDimension = account, LedgerAccount = string.Empty, PostingType = LedgerPostingType.LedgerJournal,
        TransactionCurrencyCode = "SAR", TransactionCurrencyAmount = amount, AccountingCurrencyAmount = amount,
        ReportingCurrencyAmount = amount, IsCredit = amount < 0 ? NoYes.Yes : NoYes.No,
        Text = string.Empty, PaymentReference = string.Empty
    };

    private static LedgerTrialBalanceService NewService(DbContext db) => new(new TestUnitOfWork(db), new TestUser());

    private static async Task<BalanceContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new BalanceContext(new DbContextOptionsBuilder<BalanceContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class BalanceContext(DbContextOptions<BalanceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(MainAccount), typeof(IAX.IXApi.Modules.Finance.Entities.Ledger),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry)];
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
