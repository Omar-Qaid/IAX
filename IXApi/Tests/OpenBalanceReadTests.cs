using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The customer and vendor open-balance read.
///
/// The balance is taken from CustTransOpen / VendTransOpen — the tables the payment paths settle
/// against — not recomputed over CustTrans/VendTrans, so the figure a caller sees is the figure
/// settlement will work from. An unsettled payment remainder is an open transaction too and nets
/// against the invoices. Payable amounts are credits, so a vendor balance owed is negative.
/// </summary>
public class OpenBalanceReadTests
{
    private static readonly DateTime Asof = new(2026, 11, 1);

    // ---------- Accounts receivable ----------

    [Fact]
    public async Task The_customer_balance_is_the_sum_of_the_open_rows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db, recId: 1, "INV-1", amount: 1000m, settled: 0m, due: new DateTime(2026, 10, 1));
        SeedCustomer(db, recId: 2, "INV-2", amount: 500m, settled: 200m, due: new DateTime(2026, 11, 20));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).GetOpenBalanceAsync("C-001", Asof, default);

        Assert.Equal(1300m, result.OpenAmountCur);
        Assert.Equal(1300m, result.OpenAmountMst);
        Assert.Equal(2, result.TransactionCount);
        Assert.True(result.Reconciled);
        Assert.Equal("C-001", result.AccountNum);
        Assert.Equal(Asof, result.AsOfDate);
    }

    [Fact]
    public async Task An_unsettled_payment_remainder_nets_against_the_invoices()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db, recId: 1, "INV-1", amount: 1000m, settled: 0m, due: new DateTime(2026, 10, 1));
        // An overpayment left 300 sitting open on the payment, as a credit.
        SeedCustomer(db, recId: 2, "", amount: -300m, settled: 0m, due: new DateTime(2026, 10, 15),
            type: LedgerTransType.Payment);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).GetOpenBalanceAsync("C-001", Asof, default);

        Assert.Equal(700m, result.OpenAmountCur);
        Assert.Equal(2, result.TransactionCount);
    }

    [Fact]
    public async Task Transactions_come_back_due_first_with_the_days_overdue()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db, recId: 1, "INV-LATE", amount: 100m, settled: 0m, due: new DateTime(2026, 10, 2));
        SeedCustomer(db, recId: 2, "INV-FUTURE", amount: 100m, settled: 0m, due: new DateTime(2026, 12, 1));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).GetOpenBalanceAsync("C-001", Asof, default);

        Assert.Equal("INV-LATE", result.Transactions[0].Invoice);
        Assert.Equal(30, result.Transactions[0].DaysOverdue);
        // Not yet due is zero, never negative.
        Assert.Equal("INV-FUTURE", result.Transactions[1].Invoice);
        Assert.Equal(0, result.Transactions[1].DaysOverdue);
    }

    [Fact]
    public async Task A_transaction_after_the_as_of_date_is_excluded()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db, recId: 1, "INV-1", amount: 1000m, settled: 0m, due: new DateTime(2026, 10, 1));
        SeedCustomer(db, recId: 2, "INV-LATER", amount: 400m, settled: 0m, due: new DateTime(2026, 12, 1),
            transDate: new DateTime(2026, 11, 20));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).GetOpenBalanceAsync("C-001", Asof, default);

        Assert.Equal(1000m, result.OpenAmountCur);
        Assert.Equal(1, result.TransactionCount);
    }

    [Fact]
    public async Task Omitting_the_account_reads_every_customer_in_the_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db, recId: 1, "INV-1", amount: 1000m, settled: 0m, due: new DateTime(2026, 10, 1));
        SeedCustomer(db, recId: 2, "INV-2", amount: 250m, settled: 0m, due: new DateTime(2026, 10, 1),
            account: "C-002");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var all = await NewCustService(db).GetOpenBalanceAsync(null, Asof, default);
        var one = await NewCustService(db).GetOpenBalanceAsync("C-002", Asof, default);

        Assert.Equal(1250m, all.OpenAmountCur);
        Assert.Null(all.AccountNum);
        Assert.Equal(250m, one.OpenAmountCur);
    }

    /// <summary>
    /// The payment path refuses to settle when an open row disagrees with its transaction, so the
    /// read reports that state instead of presenting a balance settlement would reject.
    /// </summary>
    [Fact]
    public async Task An_open_row_that_disagrees_with_its_transaction_is_reported_unreconciled()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db, recId: 1, "INV-1", amount: 1000m, settled: 0m, due: new DateTime(2026, 10, 1));
        await db.SaveChangesAsync();
        var open = await db.Set<CustTransOpen>().FirstAsync();
        open.AmountCur = 900m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).GetOpenBalanceAsync("C-001", Asof, default);

        Assert.False(result.Reconciled);
        // The reported figure is still the open table's, which is what settlement would read.
        Assert.Equal(900m, result.OpenAmountCur);
    }

    [Fact]
    public async Task A_customer_with_nothing_open_reads_as_zero()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var result = await NewCustService(db).GetOpenBalanceAsync("C-001", Asof, default);

        Assert.Equal(0m, result.OpenAmountCur);
        Assert.Equal(0, result.TransactionCount);
        Assert.Empty(result.Transactions);
        // Nothing to disagree, so nothing is out of reconciliation.
        Assert.True(result.Reconciled);
    }

    // ---------- Accounts payable ----------

    [Fact]
    public async Task The_vendor_balance_is_negative_and_sums_the_open_rows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, recId: 1, "VINV-1", amount: -1000m, settled: 0m, due: new DateTime(2026, 10, 1));
        SeedVendor(db, recId: 2, "VINV-2", amount: -500m, settled: -200m, due: new DateTime(2026, 11, 20));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).GetOpenBalanceAsync("V-001", Asof, default);

        Assert.Equal(-1300m, result.OpenAmountCur);
        Assert.Equal(2, result.TransactionCount);
        Assert.True(result.Reconciled);
    }

    [Fact]
    public async Task The_vendor_read_excludes_later_transactions_and_orders_by_due_date()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, recId: 1, "VINV-LATE", amount: -100m, settled: 0m, due: new DateTime(2026, 10, 2));
        SeedVendor(db, recId: 2, "VINV-SOON", amount: -100m, settled: 0m, due: new DateTime(2026, 10, 1));
        SeedVendor(db, recId: 3, "VINV-LATER", amount: -400m, settled: 0m, due: new DateTime(2026, 12, 1),
            transDate: new DateTime(2026, 11, 20));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).GetOpenBalanceAsync("V-001", Asof, default);

        Assert.Equal(-200m, result.OpenAmountCur);
        Assert.Equal("VINV-SOON", result.Transactions[0].Invoice);
        Assert.Equal(31, result.Transactions[0].DaysOverdue);
    }

    [Fact]
    public async Task A_vendor_open_row_that_disagrees_is_reported_unreconciled()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, recId: 1, "VINV-1", amount: -1000m, settled: 0m, due: new DateTime(2026, 10, 1));
        await db.SaveChangesAsync();
        var open = await db.Set<VendTransOpen>().FirstAsync();
        open.AmountCur = -900m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).GetOpenBalanceAsync("V-001", Asof, default);

        Assert.False(result.Reconciled);
        Assert.Equal(-900m, result.OpenAmountCur);
    }

    // ---------- Vendor aging ----------

    /// <summary>
    /// Each open row lands in the bucket its days overdue put it in, measured from the due date to
    /// the as-of date. The bucket totals add up to the open balance, so the aging report and the
    /// open balance read cannot disagree.
    /// </summary>
    [Fact]
    public async Task Vendor_aging_puts_each_open_row_in_its_bucket()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // Due dates against the 2026-11-01 as-of date: not yet due, 12, 31, 61 and 92 days overdue.
        SeedVendor(db, recId: 1, "VINV-FUTURE", amount: -100m, settled: 0m, due: new DateTime(2026, 11, 20));
        SeedVendor(db, recId: 2, "VINV-12", amount: -200m, settled: 0m, due: new DateTime(2026, 10, 20));
        SeedVendor(db, recId: 3, "VINV-31", amount: -300m, settled: 0m, due: new DateTime(2026, 10, 1));
        SeedVendor(db, recId: 4, "VINV-61", amount: -400m, settled: 0m, due: new DateTime(2026, 9, 1));
        SeedVendor(db, recId: 5, "VINV-92", amount: -500m, settled: 0m, due: new DateTime(2026, 8, 1));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).GetAgingAsync("V-001", Asof, default);

        Assert.Equal(5, result.Buckets.Count);
        Assert.Equal(-100m, result.Buckets[0].OpenAmountMst);   // not yet due
        Assert.Equal(-200m, result.Buckets[1].OpenAmountMst);   // 1-30
        Assert.Equal(-300m, result.Buckets[2].OpenAmountMst);   // 31-60
        Assert.Equal(-400m, result.Buckets[3].OpenAmountMst);   // 61-90
        Assert.Equal(-500m, result.Buckets[4].OpenAmountMst);   // 91+
        Assert.All(result.Buckets, bucket => Assert.Equal(1, bucket.TransactionCount));
        // The bucket boundaries are the ones the report documents.
        Assert.Equal((null, 0), (result.Buckets[0].MinimumDaysOverdue, result.Buckets[0].MaximumDaysOverdue));
        Assert.Equal((91, null), (result.Buckets[4].MinimumDaysOverdue, result.Buckets[4].MaximumDaysOverdue));
        // The buckets reconcile to the balance the open-balance read reports for the same date.
        Assert.Equal(-1500m, result.OpenAmountMst);
        Assert.Equal(result.OpenAmountMst, result.Buckets.Sum(x => x.OpenAmountMst));
        Assert.Equal((await NewVendService(db).GetOpenBalanceAsync("V-001", Asof, default)).OpenAmountMst,
            result.OpenAmountMst);
        Assert.True(result.Reconciled);
        // A single-vendor request still lists that vendor, so the shape does not change.
        Assert.Equal("V-001", Assert.Single(result.Vendors).AccountNum);
    }

    /// <summary>
    /// Omitting the vendor ages every vendor in the company and breaks the result down per vendor,
    /// so a company-wide report needs one call rather than one per vendor.
    /// </summary>
    [Fact]
    public async Task Vendor_aging_without_an_account_ages_every_vendor_and_breaks_it_down()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, recId: 1, "VINV-1", amount: -300m, settled: 0m, due: new DateTime(2026, 10, 1));
        SeedVendor(db, recId: 2, "VINV-2", amount: -100m, settled: 0m, due: new DateTime(2026, 11, 20));
        SeedVendor(db, recId: 3, "VINV-3", amount: -1000m, settled: -400m,
            due: new DateTime(2026, 8, 1), account: "V-002");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).GetAgingAsync(null, Asof, default);

        Assert.Null(result.AccountNum);
        Assert.Equal(-1000m, result.OpenAmountMst);          // -300 -100 and the -600 remainder
        Assert.Equal(3, result.TransactionCount);
        Assert.Equal(2, result.Vendors.Count);
        var first = result.Vendors[0];
        var second = result.Vendors[1];
        Assert.Equal("V-001", first.AccountNum);
        Assert.Equal(-400m, first.OpenAmountMst);
        Assert.Equal(-100m, first.Buckets[0].OpenAmountMst);  // not yet due
        Assert.Equal(-300m, first.Buckets[2].OpenAmountMst);  // 31-60
        Assert.Equal("V-002", second.AccountNum);
        // The settled part is not open, so only the remainder ages.
        Assert.Equal(-600m, second.OpenAmountMst);
        Assert.Equal(-600m, second.Buckets[4].OpenAmountMst); // 91+
        // Every vendor's buckets add up to that vendor's balance, and to the report total.
        Assert.All(result.Vendors, vendor =>
            Assert.Equal(vendor.OpenAmountMst, vendor.Buckets.Sum(x => x.OpenAmountMst)));
        Assert.Equal(result.OpenAmountMst, result.Vendors.Sum(x => x.OpenAmountMst));
    }

    // ---------- Fixture ----------

    private static void SeedCustomer(DbContext db, long recId, string invoice, decimal amount, decimal settled,
        DateTime due, DateTime? transDate = null, string account = "C-001",
        LedgerTransType type = LedgerTransType.Sales)
    {
        var date = transDate ?? new DateTime(2026, 10, 1);
        db.Add(new CustTrans
        {
            RecId = recId, DataAreaId = "dat", AccountNum = account, Invoice = invoice,
            Voucher = $"VOU-{recId}", TransDate = date, DueDate = due, CurrencyCode = "SAR",
            AmountCur = amount, AmountMst = amount, ReportingCurrencyAmount = amount,
            SettleAmountCur = settled, SettleAmountMst = settled, TransType = type
        });
        db.Add(new CustTransOpen
        {
            RecId = recId, DataAreaId = "dat", AccountNum = account, RefRecId = recId,
            TransDate = date, DueDate = due,
            AmountCur = amount - settled, AmountMst = amount - settled,
            ReportingCurrencyAmount = amount - settled
        });
    }

    private static void SeedVendor(DbContext db, long recId, string invoice, decimal amount, decimal settled,
        DateTime due, DateTime? transDate = null, string account = "V-001",
        LedgerTransType type = LedgerTransType.Purch)
    {
        var date = transDate ?? new DateTime(2026, 10, 1);
        db.Add(new VendTrans
        {
            RecId = recId, DataAreaId = "dat", AccountNum = account, Invoice = invoice,
            Voucher = $"VOU-{recId}", TransDate = date, DueDate = due, CurrencyCode = "SAR",
            AmountCur = amount, AmountMst = amount,
            SettleAmountCur = settled, SettleAmountMst = settled, TransType = type
        });
        db.Add(new VendTransOpen
        {
            RecId = recId, DataAreaId = "dat", AccountNum = account, RefRecId = recId,
            TransDate = date, DueDate = due,
            AmountCur = amount - settled, AmountMst = amount - settled
        });
    }

    private static CustTransService NewCustService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser());

    private static VendTransService NewVendService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), new TestCompany());

    private static async Task<OpenBalanceContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new OpenBalanceContext(
            new DbContextOptionsBuilder<OpenBalanceContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class OpenBalanceContext(DbContextOptions<OpenBalanceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(CustTrans), typeof(CustTransOpen), typeof(VendTrans), typeof(VendTransOpen)];

            // CustTrans and VendTrans reach the party and address graph, so everything not named
            // here is ignored outright rather than discovered one failure at a time.
            var included = new HashSet<Type>(types);
            foreach (var type in typeof(CustTrans).Assembly.GetTypes()
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
        }
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }

    private sealed class TestUser : IAX.IXApi.Infrastructure.Identity.ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    public class OpenBalanceRepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => method?.Name switch
        {
            "GetQueryable" => Context.Set<T>().AsQueryable(),
            "AddAsync" => Task.FromResult((T)args![0]!),
            "UpdateAsync" => Task.FromResult((T)args![0]!),
            _ => throw new NotSupportedException(method?.Name)
        };
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, OpenBalanceRepositoryProxy<T>>();
            ((OpenBalanceRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }
}
