using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Application.NumberSequences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// D365's *Settle open transactions* for both subledgers: applying marked open transactions to each
/// other with no payment involved.
///
/// <para>
/// Before this existed, settlement happened only inside the payment services, so a credit note and
/// a payment's unapplied remainder each wrote an open row that nothing could ever apply to an
/// invoice. They stayed open indefinitely.
/// </para>
///
/// <para>
/// Settlement posts nothing to the general ledger, and that is correct rather than missing: both
/// sides are already posted, so the account balance is already the net of the two and settling only
/// closes the open rows against each other. These tests therefore assert the subledger effect, and
/// that the <b>existing</b> settlement reversal round-trips what this writes - which is the real
/// proof the rows are the right shape.
/// </para>
/// </summary>
public class SettleOpenTransactionsTests
{
    private static readonly DateTime Date = new(2026, 10, 15);

    // ---------- Accounts receivable ----------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Manual_settlement_checks_the_exact_remaining_accounting_amount(bool matchedRemainder)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        AddCustTrans(db, 1, 1000m, LedgerTransType.Sales, "INV-1", "USD", 3750m);
        AddCustTrans(db, 2, -1000m, LedgerTransType.Payment, "PAY-1", "USD", -3750m);
        var creditRemaining = matchedRemainder ? 1249.98m : 1249.99m;
        foreach (var transaction in db.Set<CustTrans>().Local)
        {
            var open = db.Set<CustTransOpen>().Local.Single(x => x.RefRecId == transaction.RecId);
            open.AmountCur = transaction.RecId == 1 ? 333.33m : -333.33m;
            open.AmountMst = transaction.RecId == 1 ? 1249.98m : -creditRemaining;
            open.ReportingCurrencyAmount = open.AmountMst;
            transaction.SettleAmountCur = transaction.AmountCur - open.AmountCur;
            transaction.SettleAmountMst = transaction.AmountMst - open.AmountMst;
            transaction.SettleAmountReporting = transaction.SettleAmountMst;
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).SettleAsync(new("C-001", Date, [1, 2]), default);
        if (!matchedRemainder)
        {
            Assert.Equal(422, result.StatusCode);
            Assert.Contains("rounding difference", result.Message);
            Assert.Empty(await db.Set<CustSettlement>().ToListAsync());
            Assert.Equal(2, await db.Set<CustTransOpen>().CountAsync());
            return;
        }

        Assert.True(result.StatusCode == 200, result.Message);
        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        var rows = await db.Set<CustSettlement>().ToListAsync();
        Assert.Equal(1249.98m, rows.Single(x => x.TransRecId == 1).SettleAmountMst);
        Assert.Equal(0m, rows.Sum(x => x.SettleAmountMst));
        Assert.Equal(0m, rows.Sum(x => x.SettleAmountReporting));
    }

    /// <summary>
    /// The case this closes: a credit note that nothing could apply. A 1,000 invoice and a 1,000
    /// credit note both close, and the customer is left with nothing open.
    /// </summary>
    [Fact]
    public async Task A_credit_note_settles_a_customer_invoice_in_full_and_closes_both()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        AddCustTrans(db, recId: 1, amount: 1000m, LedgerTransType.Sales, "INV-1");
        AddCustTrans(db, recId: 2, amount: -1000m, LedgerTransType.Sales, "CN-1");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).SettleAsync(
            new CustSettleOpenInput("C-001", Date, [1, 2]), default);

        Assert.Equal(200, result.StatusCode);
        // Nothing is left open on either transaction.
        Assert.Empty(await db.Set<CustTransOpen>().AsNoTracking().ToListAsync());
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        var credit = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 2);
        Assert.Equal(1000m, invoice.SettleAmountCur);
        Assert.Equal(-1000m, credit.SettleAmountCur);
        Assert.Equal(CustSettlementStatus.Settled, invoice.Settlement);
        Assert.Equal(CustSettlementStatus.Settled, credit.Settlement);
        Assert.Equal(Date, invoice.Closed);
        // One pair, so two settlement rows under one voucher, both reversible.
        var rows = await db.Set<CustSettlement>().AsNoTracking().ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Single(rows.Select(x => x.SettlementVoucher).Distinct());
        Assert.All(rows, x => Assert.Equal(NoYes.Yes, x.CanBeReversed));
        // The pair nets to zero, which is why no ledger entry is needed.
        Assert.Equal(0m, rows.Sum(x => x.SettleAmountCur));
    }

    /// <summary>
    /// A credit note smaller than the invoice settles partly: the invoice stays open for the
    /// difference and the credit note closes.
    /// </summary>
    [Fact]
    public async Task A_smaller_credit_note_partly_settles_the_invoice_and_leaves_the_difference_open()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        AddCustTrans(db, recId: 1, amount: 1000m, LedgerTransType.Sales, "INV-1");
        AddCustTrans(db, recId: 2, amount: -400m, LedgerTransType.Sales, "CN-1");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).SettleAsync(
            new CustSettleOpenInput("C-001", Date, [1, 2]), default);

        Assert.Equal(200, result.StatusCode);
        var open = Assert.Single(await db.Set<CustTransOpen>().AsNoTracking().ToListAsync());
        Assert.Equal(1, open.RefRecId);
        Assert.Equal(600m, open.AmountCur);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        Assert.Equal(400m, invoice.SettleAmountCur);
        // The invoice is not closed: it still owes 600.
        Assert.Equal(default, invoice.Closed);
        Assert.Equal(CustSettlementStatus.None, invoice.Settlement);
        // The open row still satisfies the invariant the reversal rebuilds from.
        Assert.Equal(invoice.AmountCur - invoice.SettleAmountCur, open.AmountCur);
    }

    /// <summary>
    /// The real proof the rows are shaped right: the <b>existing</b> reversal service, written for
    /// payment settlements, puts everything back without any change of its own.
    /// </summary>
    [Fact]
    public async Task The_existing_reversal_round_trips_a_settlement_this_service_wrote()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        AddCustTrans(db, recId: 1, amount: 1000m, LedgerTransType.Sales, "INV-1");
        AddCustTrans(db, recId: 2, amount: -400m, LedgerTransType.Sales, "CN-1");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var settled = await NewCustService(db).SettleAsync(
            new CustSettleOpenInput("C-001", Date, [1, 2]), default);
        Assert.Equal(200, settled.StatusCode);
        var voucher = await db.Set<CustSettlement>().AsNoTracking()
            .Select(x => x.SettlementVoucher).FirstAsync();
        db.ChangeTracker.Clear();

        var reversed = await NewCustReversal(db).ReverseAsync(voucher, Date.AddDays(1), default);

        Assert.Equal(200, reversed.StatusCode);
        // Both transactions are open again for their full amounts.
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        var credit = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 2);
        Assert.Equal(0m, invoice.SettleAmountCur);
        Assert.Equal(0m, credit.SettleAmountCur);
        Assert.Equal(1000m, (await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 1)).AmountCur);
        Assert.Equal(-400m, (await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 2)).AmountCur);
    }

    /// <summary>Marked transactions on the same side have nothing to settle against each other.</summary>
    [Fact]
    public async Task Two_invoices_cannot_be_settled_against_each_other()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        AddCustTrans(db, recId: 1, amount: 1000m, LedgerTransType.Sales, "INV-1");
        AddCustTrans(db, recId: 2, amount: 500m, LedgerTransType.Sales, "INV-2");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).SettleAsync(
            new CustSettleOpenInput("C-001", Date, [1, 2]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("all on the same side", result.Message);
        Assert.Empty(await db.Set<CustSettlement>().AsNoTracking().ToListAsync());
    }

    /// <summary>
    /// Two transactions in one foreign currency but at different rates would realize an exchange
    /// difference, which this path cannot post, so it refuses instead of losing the difference.
    /// </summary>
    [Fact]
    public async Task Transactions_posted_at_different_rates_are_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        // 1,000 USD booked at 3.75, then a 1,000 USD credit booked at 3.60.
        AddCustTrans(db, recId: 1, amount: 1000m, LedgerTransType.Sales, "INV-1", currency: "USD", accounting: 3750m);
        AddCustTrans(db, recId: 2, amount: -1000m, LedgerTransType.Sales, "CN-1", currency: "USD", accounting: -3600m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).SettleAsync(
            new CustSettleOpenInput("C-001", Date, [1, 2]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("different exchange rates", result.Message);
        Assert.Empty(await db.Set<CustSettlement>().AsNoTracking().ToListAsync());
    }

    /// <summary>A transaction with no open row is not available to settle.</summary>
    [Fact]
    public async Task A_transaction_that_is_not_open_is_refused_by_name()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        AddCustTrans(db, recId: 1, amount: 1000m, LedgerTransType.Sales, "INV-1");
        AddCustTrans(db, recId: 2, amount: -400m, LedgerTransType.Sales, "CN-1", open: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).SettleAsync(
            new CustSettleOpenInput("C-001", Date, [1, 2]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("is not an open transaction", result.Message);
    }

    // ---------- Accounts payable ----------

    /// <summary>
    /// The payable mirror: a vendor credit note applied to a vendor invoice. A vendor invoice is a
    /// credit, so its amounts are negative and the credit note's are positive.
    /// </summary>
    [Fact]
    public async Task A_vendor_credit_note_settles_a_vendor_invoice_in_full()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db);
        AddVendTrans(db, recId: 1, amount: -1000m, LedgerTransType.Purch, "VINV-1");
        AddVendTrans(db, recId: 2, amount: 1000m, LedgerTransType.Purch, "VCN-1");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).SettleAsync(
            new VendSettleOpenInput("V-001", Date, [1, 2]), default);

        Assert.Equal(200, result.StatusCode);
        Assert.Empty(await db.Set<VendTransOpen>().AsNoTracking().ToListAsync());
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        var credit = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 2);
        // The payable path subtracts on the invoice side and adds on the offsetting side.
        Assert.Equal(-1000m, invoice.SettleAmountCur);
        Assert.Equal(1000m, credit.SettleAmountCur);
        Assert.Equal(Date, invoice.Closed);
        Assert.Equal(Date, credit.Closed);
        var row = Assert.Single(await db.Set<VendSettlement>().AsNoTracking().ToListAsync());
        // AmountCur is a positive magnitude, as VendPaymentService writes it.
        Assert.Equal(1000m, row.AmountCur);
        Assert.Equal(1, row.InvoiceTransRecId);
        Assert.Equal(2, row.PaymentTransRecId);
        Assert.Equal(NoYes.Yes, row.CanBeReversed);
    }

    /// <summary>The existing vendor reversal round-trips what this service wrote.</summary>
    [Fact]
    public async Task The_existing_vendor_reversal_round_trips_a_settlement_this_service_wrote()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db);
        AddVendTrans(db, recId: 1, amount: -1000m, LedgerTransType.Purch, "VINV-1");
        AddVendTrans(db, recId: 2, amount: 400m, LedgerTransType.Purch, "VCN-1");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var settled = await NewVendService(db).SettleAsync(
            new VendSettleOpenInput("V-001", Date, [1, 2]), default);
        Assert.Equal(200, settled.StatusCode);
        var voucher = await db.Set<VendSettlement>().AsNoTracking().Select(x => x.Voucher).FirstAsync();
        db.ChangeTracker.Clear();

        var reversed = await NewVendReversal(db).ReverseAsync(voucher, Date.AddDays(1), default);

        Assert.Equal(200, reversed.StatusCode);
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        var credit = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 2);
        Assert.Equal(0m, invoice.SettleAmountCur);
        Assert.Equal(0m, credit.SettleAmountCur);
        Assert.Equal(-1000m, (await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 1)).AmountCur);
        Assert.Equal(400m, (await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 2)).AmountCur);
    }

    /// <summary>One marked transaction cannot be settled against itself.</summary>
    [Fact]
    public async Task A_single_marked_transaction_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomer(db);
        AddCustTrans(db, recId: 1, amount: 1000m, LedgerTransType.Sales, "INV-1");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).SettleAsync(
            new CustSettleOpenInput("C-001", Date, [1]), default);

        Assert.Equal(400, result.StatusCode);
        Assert.Contains("at least two marked transactions", result.Message);
    }

    // ---------- Fixture ----------

    private static void SeedCustomer(DbContext db) =>
        db.Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CurrencyCode = "SAR" });

    private static void SeedVendor(DbContext db) =>
        db.Add(new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", Currency = "SAR" });

    /// <summary>
    /// A posted, wholly unsettled customer transaction and its open row. <paramref name="accounting"/>
    /// defaults to the transaction amount, which is the same-currency case.
    /// </summary>
    private static void AddCustTrans(DbContext db, long recId, decimal amount, LedgerTransType type,
        string invoice, string currency = "SAR", decimal? accounting = null, bool open = true)
    {
        var mst = accounting ?? amount;
        db.Add(new CustTrans
        {
            RecId = recId, DataAreaId = "dat", AccountNum = "C-001", Invoice = invoice, Voucher = invoice,
            TransDate = Date, DueDate = Date, CurrencyCode = currency,
            AmountCur = amount, AmountMst = mst, ReportingCurrencyAmount = mst, TransType = type
        });
        if (open)
            db.Add(new CustTransOpen
            {
                RecId = recId, DataAreaId = "dat", AccountNum = "C-001", RefRecId = recId,
                TransDate = Date, DueDate = Date,
                AmountCur = amount, AmountMst = mst, ReportingCurrencyAmount = mst
            });
    }

    private static void AddVendTrans(DbContext db, long recId, decimal amount, LedgerTransType type,
        string invoice, string currency = "SAR", decimal? accounting = null, bool open = true)
    {
        var mst = accounting ?? amount;
        db.Add(new VendTrans
        {
            RecId = recId, DataAreaId = "dat", AccountNum = "V-001", Invoice = invoice, Voucher = invoice,
            TransDate = Date, DueDate = Date, CurrencyCode = currency,
            AmountCur = amount, AmountMst = mst, TransType = type
        });
        if (open)
            db.Add(new VendTransOpen
            {
                RecId = recId, DataAreaId = "dat", AccountNum = "V-001", RefRecId = recId,
                TransDate = Date, DueDate = Date, AmountCur = amount, AmountMst = mst
            });
    }

    private static CustSettleOpenTransactionsService NewCustService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, SettleSequenceProxy>());

    private static CustSettlementReversalService NewCustReversal(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, SettleSequenceProxy>());

    private static VendSettleOpenTransactionsService NewVendService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, SettleSequenceProxy>());

    private static VendSettlementReversalService NewVendReversal(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany());

    private static async Task<SettleContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new SettleContext(new DbContextOptionsBuilder<SettleContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class SettleContext(DbContextOptions<SettleContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement), typeof(CustTable),
                typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(VendTable),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry), typeof(LedgerJournalTrans)];

            // CustTable and VendTable reach the party and address graph, so everything not named
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

    public class SettleRepositoryProxy<T> : DispatchProxy where T : class
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
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, SettleRepositoryProxy<T>>();
            ((SettleRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }

    /// <summary>Hands out sequential settlement vouchers without a number-sequence table.</summary>
    public class SettleSequenceProxy : DispatchProxy
    {
        private int _next;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(ISysNumberSequenceService.NextAsync))
                return Task.FromResult(new NextSequenceResultDto
                    { EntityName = "CustSettlement", Value = ++_next, Code = $"SET-{_next:D6}" });
            throw new NotSupportedException(targetMethod?.Name);
        }
    }
}
