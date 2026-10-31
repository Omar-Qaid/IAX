using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using JournalHeader = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Tests;

/// <summary>
/// Validate and Simulate from the core cycle (section 5 step 7, section 6.6, acceptance test T15:
/// "Simulate posting — no final posted accounting created by simulation").
///
/// Validate reports what the static checks see. Simulate runs the real posting path and rolls it
/// back, so its answer comes from the rules that actually post rather than from a second
/// implementation that could drift from them.
/// </summary>
public class LedgerJournalValidateSimulateTests
{
    /// <summary>
    /// T15: the whole point. Simulation reports what would post and leaves nothing behind — no
    /// posted entry, no voucher association, and the journal still unposted and editable.
    /// </summary>
    [Fact]
    public async Task Simulating_reports_what_would_post_and_keeps_no_posted_accounting()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 400m), Line(2, "V1", bank: false, credit: 400m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewService(db).SimulateAsync(1, default);

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.True(result!.WouldPost);
        Assert.Null(result.Refusal);
        Assert.False(result.Posted);
        Assert.Equal(1, result.Vouchers);
        Assert.Equal(2, result.AccountEntries);
        Assert.Equal(400m, result.TotalDebit);
        Assert.Equal(400m, result.TotalCredit);
        // The entries it would create are reported, which is what makes a simulation reviewable.
        Assert.Equal(2, result.Entries.Count);
        Assert.Contains(result.Entries, x => x.LedgerAccount == "110100" && x.AccountingCurrencyAmount == 400m);
        Assert.Contains(result.Entries, x => x.LedgerAccount == "310100" && x.AccountingCurrencyAmount == -400m);

        // Nothing survived the call.
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
        Assert.Empty(await db.Set<SubledgerVoucherGeneralJournalEntry>().AsNoTracking().ToListAsync());
        var header = await db.Set<JournalHeader>().AsNoTracking().SingleAsync();
        Assert.Equal(NoYes.No, header.Posted);
        Assert.Equal(default, header.PostedDateTime);
        Assert.All(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync(),
            x => Assert.Equal(NoYes.No, x.NoEdit));
    }

    // A simulation that leaked its in-memory mutations would post on the next save, so the
    // change tracker must be clean afterwards.
    [Fact]
    public async Task Simulating_leaves_no_tracked_changes_and_the_journal_still_posts_afterwards()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 250m), Line(2, "V1", bank: false, credit: 250m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewService(db);

        await service.SimulateAsync(1, default);
        Assert.Empty(db.ChangeTracker.Entries());
        await db.SaveChangesAsync(); // Would persist a leaked "Posted" if one had survived.
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());

        // Simulating twice is safe, and the journal still posts for real afterwards.
        await service.SimulateAsync(1, default);
        db.ChangeTracker.Clear();
        var (posted, error) = await service.PostAsync(1, default);
        Assert.Null(error);
        Assert.Equal(1, posted!.VouchersPosted);
        db.ChangeTracker.Clear();
        Assert.Single(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
    }

    // A refusal is the useful answer, reported rather than thrown.
    [Fact]
    public async Task Simulating_an_unpostable_journal_reports_the_refusal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        // Out of balance, so the posting path refuses it.
        db.AddRange(Line(1, "V1", bank: true, debit: 400m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewService(db).SimulateAsync(1, default);

        Assert.Null(error);
        Assert.False(result!.WouldPost);
        Assert.Contains("unbalanced", result.Refusal);
        Assert.Empty(result.Entries);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Validating_combines_the_setup_and_arithmetic_checks_without_posting()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 400m), Line(2, "V1", bank: false, credit: 400m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var valid = await NewService(db).ValidateAsync(1, default);
        Assert.NotNull(valid);
        Assert.True(valid!.Valid);
        Assert.Empty(valid.Errors);
        // Validate never claims posting will succeed.
        Assert.Contains("Simulate posting for an authoritative answer", valid.Scope);
        Assert.NotEmpty(valid.UnverifiedChecks);

        db.ChangeTracker.Clear();
        var line = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.RecId == 2);
        line.AmountCurCredit = 100m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var invalid = await NewService(db).ValidateAsync(1, default);
        Assert.False(invalid!.Valid);
        Assert.Contains(invalid.Errors, x => x.Contains("unbalanced"));
        // Reads only: no posted accounting either way.
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task An_unknown_journal_is_reported_as_missing_by_both()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewService(db);

        Assert.Null(await service.ValidateAsync(404, default));
        var (result, error) = await service.SimulateAsync(404, default);
        Assert.Null(result);
        Assert.Null(error);
    }

    private static LedgerJournalTableService NewService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser());

    private static LedgerJournalTrans Line(long recId, string voucher, bool bank,
        decimal debit = 0m, decimal credit = 0m) => new()
        {
            RecId = recId, DataAreaId = "dat", Company = "dat", JournalNum = "J1", LineNum = recId,
            Voucher = voucher, TransDate = new DateTime(2026, 1, 1), CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Ledger, LedgerDimension = bank ? 1 : 2,
            AmountCurDebit = debit, AmountCurCredit = credit,
            Txt = bank ? "Bank ledger control" : "Share capital"
        };

    private static void Seed(DbContext db)
    {
        db.AddRange(
            new JournalHeader { RecId = 1, DataAreaId = "dat", JournalNum = "J1", JournalName = "GEN" },
            new LedgerJournalName { RecId = 1, DataAreaId = "dat", JournalName = "GEN" },
            new Ledger { RecId = 1, DataAreaId = "dat", ChartOfAccounts = 10, FiscalCalendar = 1, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) },
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new MainAccount { RecId = 1, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "110100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "310100" },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "110100", DisplayValue = "110100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "310100", DisplayValue = "310100" });
    }

    private static async Task<SimulateContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new SimulateContext(new DbContextOptionsBuilder<SimulateContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class SimulateContext(DbContextOptions<SimulateContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(JournalHeader), typeof(LedgerJournalName), typeof(LedgerJournalTrans),
                typeof(CompanyInfo), typeof(MainAccountLegalEntity), typeof(IAX.IXApi.Modules.Finance.Entities.LedgerParameters),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry),
                typeof(SubledgerVoucherGeneralJournalEntry), typeof(ExchangeRateType),
                typeof(ExchangeRateCurrencyPair), typeof(ExchangeRate)];
            builder.Ignore<DirPartyTable>();
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
        private IDbContextTransaction? _tx;
        public DbContext Context => context;
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, SimulateRepositoryProxy<T>>();
            ((SimulateRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }

    public class SimulateRepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            method?.Name == "GetQueryable" ? Context.Set<T>().AsQueryable() : throw new NotSupportedException(method?.Name);
    }
}
