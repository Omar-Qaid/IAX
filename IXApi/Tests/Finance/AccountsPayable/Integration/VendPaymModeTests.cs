using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Application.NumberSequences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The vendor method of payment, VendPaymModeTable.
///
/// A vendor payment could name any method of payment it liked: the string was written to
/// LedgerJournalTrans and BankAccountTrans with nothing to point at, and VendTable.PaymMode named
/// a method that did not have to exist. The receivable side has always validated the customer's
/// method against CustPaymModeTable, on the sales order and the free text invoice. These tests pin
/// the payable mirror: the vendor's own default applies when the payment names none, and an
/// unknown method is refused.
/// </summary>
public class VendPaymModeTests
{
    [Fact]
    public async Task A_payment_method_that_is_not_set_up_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var result = await NewService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 100m, "req-1", Date, PaymMode: "NOSUCH"), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Equal("The method of payment was not found in the selected company.", result.Message);
    }

    [Fact]
    public async Task A_payment_method_that_is_set_up_posts_and_is_carried_onto_the_journal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddMode(db, "CHECK");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 100m, "req-1", Date, PaymMode: "CHECK"), default);

        Assert.Equal(200, result.StatusCode);
        var line = Assert.Single(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync());
        Assert.Equal("CHECK", line.PaymMode);
        var movement = Assert.Single(await db.Set<BankAccountTrans>().AsNoTracking().ToListAsync());
        Assert.Equal("CHECK", movement.PaymentMode);
    }

    /// <summary>
    /// The vendor master's default, the way the free text invoice takes the customer's. A payment
    /// that names no method is not a payment with no method.
    /// </summary>
    [Fact]
    public async Task The_vendors_own_method_applies_when_the_payment_names_none()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddMode(db, "TRANSFER");
        db.Add(new VendTable { RecId = 2, DataAreaId = "dat", AccountNum = "V-002", Currency = "SAR", PaymMode = "TRANSFER" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewService(db).PostAsync(
            new VendPaymentPostDto("V-002", "BANK-1", 100m, "req-1", Date), default);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("TRANSFER", Assert.Single(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync()).PaymMode);
    }

    /// <summary>A vendor whose default method no longer exists is refused rather than posted blind.</summary>
    [Fact]
    public async Task A_vendor_default_that_is_not_set_up_is_refused_too()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Add(new VendTable { RecId = 2, DataAreaId = "dat", AccountNum = "V-002", Currency = "SAR", PaymMode = "GONE" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewService(db).PostAsync(
            new VendPaymentPostDto("V-002", "BANK-1", 100m, "req-1", Date), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Equal("The method of payment was not found in the selected company.", result.Message);
    }

    /// <summary>
    /// A payment naming no method, for a vendor with no default, still posts: the method of payment
    /// is setup, not a requirement, and making it one would refuse payments that post today.
    /// </summary>
    [Fact]
    public async Task No_method_anywhere_still_posts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var result = await NewService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 100m, "req-1", Date), default);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("", Assert.Single(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync()).PaymMode);
    }

    [Fact]
    public async Task A_second_method_with_the_same_code_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddMode(db, "CHECK");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        AddMode(db, "CHECK", recId: 9);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    // ---------- Fixture ----------

    private static readonly DateTime Date = new(2026, 1, 10);

    private static void AddMode(DbContext db, string code, long recId = 1) =>
        db.Add(new VendPaymModeTable
        {
            RecId = recId, DataAreaId = "dat", PaymMode = code, Name = code,
            AccountType = LedgerJournalACType.Bank
        });

    private static VendPaymentService NewService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, PaymModeSequenceProxy>());

    private static async Task<PaymModeContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PaymModeContext(
            new DbContextOptionsBuilder<PaymModeContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new Ledger { RecId = 1, DataAreaId = "dat", ChartOfAccounts = 10, FiscalCalendar = 1, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) },
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new MainAccount { RecId = 1, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "110100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "210100" },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "110100", DisplayValue = "110100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "210100", DisplayValue = "210100" },
            new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", Currency = "SAR" },
            new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 },
            new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 1,
                CurrencyCode = "SAR", BankAccountStatus = BankAccountStatus.Active });
        // Posting requires an active structure assigned to the ledger.
        TestAccountStructures.AssignPermissive(db, "dat", 1);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class PaymModeContext(DbContextOptions<PaymModeContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(VendPaymModeTable), typeof(VendParameters),
                typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(VendInvoiceJour), typeof(VendTable),
                typeof(VendLedgerAccounts),
                typeof(LedgerJournalTable), typeof(LedgerJournalTrans),
                typeof(BankAccountTable), typeof(BankAccountTrans),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(MainAccountLegalEntity), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry),
                typeof(SubledgerVoucherGeneralJournalEntry),
                .. TestAccountStructures.Types];

            var included = new HashSet<Type>(types);
            foreach (var type in typeof(VendTrans).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(IAX.IXApi.Shared.Domain.Entities.Entity<long>).IsAssignableFrom(t)
                    && !included.Contains(t)))
                builder.Ignore(type);

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
            builder.Entity<VendPaymModeTable>().HasIndex(x => new { x.DataAreaId, x.PaymMode }).IsUnique();
        }
    }

    private sealed class TestUser : IAX.IXApi.Infrastructure.Identity.ICurrentUserService
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
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, PaymModeRepositoryProxy<T>>();
            ((PaymModeRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }

    public class PaymModeRepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => method?.Name switch
        {
            "GetQueryable" => Context.Set<T>().AsQueryable(),
            _ => throw new NotSupportedException(method?.Name)
        };
    }

    public class PaymModeSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name != nameof(ISysNumberSequenceService.NextAsync))
                throw new NotSupportedException(method?.Name);
            var entity = (string)args![0]!;
            _n++;
            return Task.FromResult(new NextSequenceResultDto
                { EntityName = entity, Value = _n, Code = $"{entity}-{_n:D4}" });
        }
    }
}
