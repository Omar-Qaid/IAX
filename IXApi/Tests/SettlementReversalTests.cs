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
/// Settlement reversal for both subledgers.
///
/// Settlement posts nothing to the general ledger in this module — the payment's voucher carries
/// Dr Bank / Cr Customer, and settlement only moves the open receivable or payable. So reversing a
/// settlement has nothing to counter-post, and these tests assert the subledger restore instead:
/// the originals are kept, offsetting rows are written, and CustTrans/VendTrans and their open rows
/// go back to what they were. The restore is exact because it is rebuilt from the invariant the
/// payment paths already assert: open equals AmountCur less SettleAmountCur.
/// </summary>
public class SettlementReversalTests
{
    // ---------- Accounts receivable ----------

    [Fact]
    public async Task A_customer_settlement_cannot_be_reversed_before_its_original_date()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerSettlement(db, settled: 1000m, invoiceAmount: 1000m, openRemains: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var original = await db.Set<CustSettlement>().AsNoTracking().Where(x => x.SettlementVoucher == "CSET-000001")
            .Select(x => x.TransDate).FirstAsync();
        var service = NewCustService(db);
        var refused = await service.ReverseAsync("CSET-000001", original.AddDays(-1), default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("precedes", refused.Message);
        Assert.Equal(2, await db.Set<CustSettlement>().CountAsync());
        Assert.All(await db.Set<CustSettlement>().AsNoTracking().ToListAsync(), row => Assert.Equal(NoYes.Yes, row.CanBeReversed));
        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        Assert.Equal(1000m, (await db.Set<CustTrans>().SingleAsync(x => x.RecId == 1)).SettleAmountCur);
        Assert.Equal(200, (await service.ReverseAsync("CSET-000001", original, default)).StatusCode);
    }

    [Fact]
    public async Task Reversing_a_full_settlement_puts_the_invoice_back_in_the_open_table()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // A 1000 invoice settled in full: its CustTransOpen row was removed by the payment path.
        SeedCustomerSettlement(db, settled: 1000m, invoiceAmount: 1000m, openRemains: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).ReverseAsync("CSET-000001", new DateTime(2026, 11, 1), default);

        Assert.Equal(200, result.StatusCode);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        Assert.Equal(0m, invoice.SettleAmountCur);
        Assert.Equal(default, invoice.Closed);
        Assert.Equal(CustSettlementStatus.None, invoice.Settlement);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 1);
        Assert.Equal(1000m, open.AmountCur);
    }

    [Fact]
    public async Task Reversing_a_partial_settlement_restores_the_open_amount()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // 400 of a 1000 invoice settled, so 600 stayed open.
        SeedCustomerSettlement(db, settled: 400m, invoiceAmount: 1000m, openRemains: true);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).ReverseAsync("CSET-000001", null, default);

        Assert.Equal(200, result.StatusCode);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        Assert.Equal(0m, invoice.SettleAmountCur);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 1);
        Assert.Equal(1000m, open.AmountCur);
    }

    [Fact]
    public async Task The_original_rows_are_kept_and_offsetting_rows_are_written()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerSettlement(db, settled: 1000m, invoiceAmount: 1000m, openRemains: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await NewCustService(db).ReverseAsync("CSET-000001", null, default);

        var original = await db.Set<CustSettlement>().AsNoTracking()
            .Where(x => x.SettlementVoucher == "CSET-000001").ToListAsync();
        Assert.Equal(2, original.Count);
        Assert.All(original, row => Assert.Equal(NoYes.No, row.CanBeReversed));

        var reversal = await db.Set<CustSettlement>().AsNoTracking()
            .Where(x => x.SettlementVoucher != "CSET-000001").ToListAsync();
        Assert.Equal(2, reversal.Count);
        Assert.All(reversal, row => Assert.Equal(NoYes.No, row.CanBeReversed));
        // Every original amount has its mirror, so the two vouchers net to nothing.
        Assert.Equal(0m, original.Concat(reversal).Sum(x => x.SettleAmountCur));
        Assert.Equal(0m, original.Concat(reversal).Sum(x => x.SettleAmountReporting));
    }

    [Fact]
    public async Task A_settlement_cannot_be_reversed_twice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerSettlement(db, settled: 1000m, invoiceAmount: 1000m, openRemains: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewCustService(db);

        Assert.Equal(200, (await service.ReverseAsync("CSET-000001", null, default)).StatusCode);
        var second = await service.ReverseAsync("CSET-000001", null, default);

        Assert.Equal(422, second.StatusCode);
        Assert.Contains("already reversed", second.Message);
        // The first reversal stands; the refusal changed nothing.
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 1);
        Assert.Equal(1000m, open.AmountCur);
    }

    /// <summary>
    /// Settlements written before this feature default to CanBeReversed = No, so they are refused
    /// rather than reversed on an assumption about what they did.
    /// </summary>
    [Fact]
    public async Task A_settlement_not_marked_reversible_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerSettlement(db, settled: 1000m, invoiceAmount: 1000m, openRemains: false, reversible: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).ReverseAsync("CSET-000001", null, default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("not marked reversible", result.Message);
    }

    /// <summary>
    /// A cash discount, exchange adjustment or penny difference would have posted to the ledger,
    /// and this path posts no counter-voucher, so it refuses instead of leaving the ledger behind.
    /// </summary>
    [Theory]
    [InlineData("discount")]
    [InlineData("exchange")]
    [InlineData("penny")]
    public async Task A_settlement_with_a_ledger_effect_is_refused(string kind)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerSettlement(db, settled: 1000m, invoiceAmount: 1000m, openRemains: false);
        await db.SaveChangesAsync();
        var row = await db.Set<CustSettlement>().FirstAsync(x => x.SettleAmountCur > 0);
        switch (kind)
        {
            case "discount": row.UtilizedCashDisc = 10m; break;
            case "exchange": row.ExchAdjustment = 10m; break;
            case "penny": row.PennyDiff = 0.01m; break;
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustService(db).ReverseAsync("CSET-000001", null, default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("cannot be reversed by this path", result.Message);
    }

    [Fact]
    public async Task An_unknown_or_empty_customer_settlement_voucher_is_rejected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewCustService(db);

        Assert.Equal(404, (await service.ReverseAsync("CSET-999999", null, default)).StatusCode);
        Assert.Equal(400, (await service.ReverseAsync("   ", null, default)).StatusCode);
    }

    // ---------- Accounts payable ----------

    [Fact]
    public async Task Reversing_a_vendor_settlement_puts_the_invoice_back_in_the_open_table()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // A vendor invoice is a credit, so amounts are negative: -1000 settled in full by 1000 paid.
        SeedVendorSettlement(db, settled: 1000m, invoiceAmount: -1000m, openRemains: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).ReverseAsync("VOU-000001", new DateTime(2026, 11, 1), default);

        Assert.Equal(200, result.StatusCode);
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        Assert.Equal(0m, invoice.SettleAmountCur);
        Assert.Equal(default, invoice.Closed);
        var open = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 1);
        Assert.Equal(-1000m, open.AmountCur);
    }

    [Fact]
    public async Task Reversing_a_partial_vendor_settlement_restores_the_open_amount()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorSettlement(db, settled: 400m, invoiceAmount: -1000m, openRemains: true);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).ReverseAsync("VOU-000001", null, default);

        Assert.Equal(200, result.StatusCode);
        var open = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 1);
        Assert.Equal(-1000m, open.AmountCur);
    }

    [Fact]
    public async Task The_vendor_reversal_keeps_the_original_and_writes_an_offsetting_row()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorSettlement(db, settled: 1000m, invoiceAmount: -1000m, openRemains: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await NewVendService(db).ReverseAsync("VOU-000001", null, default);

        var rows = await db.Set<VendSettlement>().AsNoTracking().ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(0m, rows.Sum(x => x.AmountCur));
        Assert.All(rows, row => Assert.Equal(NoYes.No, row.CanBeReversed));
        Assert.Contains(rows, row => row.Voucher == "VOU-000001-REV");
    }

    [Fact]
    public async Task A_vendor_settlement_cannot_be_reversed_twice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorSettlement(db, settled: 1000m, invoiceAmount: -1000m, openRemains: false);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewVendService(db);

        Assert.Equal(200, (await service.ReverseAsync("VOU-000001", null, default)).StatusCode);
        var second = await service.ReverseAsync("VOU-000001", null, default);

        Assert.Equal(422, second.StatusCode);
        Assert.Contains("already reversed", second.Message);
    }

    [Fact]
    public async Task An_unknown_or_empty_vendor_settlement_voucher_is_rejected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewVendService(db);

        Assert.Equal(404, (await service.ReverseAsync("VOU-999999", null, default)).StatusCode);
        Assert.Equal(400, (await service.ReverseAsync("", null, default)).StatusCode);
    }

    // ---------- Fixture ----------

    /// <summary>
    /// Builds the state the customer payment path leaves behind: an invoice with its settled
    /// amount, the payment, the settlement pair, and whichever open rows survived.
    /// </summary>
    private static void SeedCustomerSettlement(DbContext db, decimal settled, decimal invoiceAmount,
        bool openRemains, bool reversible = true)
    {
        var date = new DateTime(2026, 10, 1);
        db.Add(new CustTrans
        {
            RecId = 1, DataAreaId = "dat", AccountNum = "C-001", Invoice = "INV-1", TransDate = date, DueDate = date,
            CurrencyCode = "SAR", AmountCur = invoiceAmount, AmountMst = invoiceAmount,
            ReportingCurrencyAmount = invoiceAmount, SettleAmountCur = settled, SettleAmountMst = settled,
            TransType = LedgerTransType.Sales,
            Closed = openRemains ? default : date,
            Settlement = openRemains ? CustSettlementStatus.None : CustSettlementStatus.Settled,
            LastSettleDate = date, LastSettleVoucher = "VOU-1", LastSettleAccountNum = "C-001"
        });
        db.Add(new CustTrans
        {
            RecId = 2, DataAreaId = "dat", AccountNum = "C-001", TransDate = date, DueDate = date,
            CurrencyCode = "SAR", AmountCur = -settled, AmountMst = -settled, ReportingCurrencyAmount = -settled,
            SettleAmountCur = -settled, SettleAmountMst = -settled, TransType = LedgerTransType.Payment,
            Closed = date, Settlement = CustSettlementStatus.Settled
        });
        if (openRemains)
            db.Add(new CustTransOpen
            {
                RecId = 1, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 1, TransDate = date, DueDate = date,
                AmountCur = invoiceAmount - settled, AmountMst = invoiceAmount - settled,
                ReportingCurrencyAmount = invoiceAmount - settled
            });
        var mark = reversible ? NoYes.Yes : NoYes.No;
        db.Add(new CustSettlement
        {
            RecId = 1, DataAreaId = "dat", AccountNum = "C-001", TransDate = date, SettlementVoucher = "CSET-000001",
            TransCompany = "dat", TransType = LedgerTransType.Sales, TransRecId = 1, OffsetRecId = 2,
            OffsetAccountNum = "C-001", OffsetCompany = "dat", SettleAmountCur = settled, SettleAmountMst = settled,
            SettleAmountReporting = settled, DueDate = date, ClosedDate = date, CanBeReversed = mark
        });
        db.Add(new CustSettlement
        {
            RecId = 2, DataAreaId = "dat", AccountNum = "C-001", TransDate = date, SettlementVoucher = "CSET-000001",
            TransCompany = "dat", TransType = LedgerTransType.Payment, TransRecId = 2, OffsetRecId = 1,
            OffsetAccountNum = "C-001", OffsetCompany = "dat", SettleAmountCur = -settled, SettleAmountMst = -settled,
            SettleAmountReporting = -settled, DueDate = date, ClosedDate = date, CanBeReversed = mark
        });
    }

    private static void SeedVendorSettlement(DbContext db, decimal settled, decimal invoiceAmount, bool openRemains)
    {
        var date = new DateTime(2026, 10, 1);
        db.Add(new VendTrans
        {
            RecId = 1, DataAreaId = "dat", AccountNum = "V-001", Invoice = "VINV-1", TransDate = date, DueDate = date,
            CurrencyCode = "SAR", AmountCur = invoiceAmount, AmountMst = invoiceAmount,
            // The payable path subtracts on the invoice side, so a settled credit carries a
            // negative settled amount and the open row is AmountCur less that.
            SettleAmountCur = -settled, SettleAmountMst = -settled, TransType = LedgerTransType.Purch,
            Closed = openRemains ? default : date
        });
        db.Add(new VendTrans
        {
            RecId = 2, DataAreaId = "dat", AccountNum = "V-001", TransDate = date, DueDate = date, CurrencyCode = "SAR",
            AmountCur = settled, AmountMst = settled, SettleAmountCur = settled, SettleAmountMst = settled,
            TransType = LedgerTransType.Payment, Closed = date
        });
        if (openRemains)
            db.Add(new VendTransOpen
            {
                RecId = 1, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 1, TransDate = date, DueDate = date,
                AmountCur = invoiceAmount + settled, AmountMst = invoiceAmount + settled
            });
        db.Add(new VendSettlement
        {
            RecId = 1, DataAreaId = "dat", AccountNum = "V-001", InvoiceTransRecId = 1, PaymentTransRecId = 2,
            TransDate = date, Voucher = "VOU-000001", AmountCur = settled, AmountMst = settled,
            CanBeReversed = NoYes.Yes
        });
    }

    private static CustSettlementReversalService NewCustService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, ReversalSequenceProxy>());

    private static VendSettlementReversalService NewVendService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany());

    private static async Task<SettlementReversalContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new SettlementReversalContext(
            new DbContextOptionsBuilder<SettlementReversalContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class SettlementReversalContext(DbContextOptions<SettlementReversalContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement),
                typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(VendInvoiceJour), typeof(GeneralJournalEntry)];

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

    public class ReversalSequenceProxy : DispatchProxy
    {
        private int _n = 1;
        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            method?.Name == nameof(ISysNumberSequenceService.NextAsync)
                ? Task.FromResult(new NextSequenceResultDto
                    { EntityName = "CustSettlement", Value = ++_n, Code = $"CSET-{_n:D6}" })
                : throw new NotSupportedException(method?.Name);
    }

    public class ReversalRepositoryProxy<T> : DispatchProxy where T : class
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
            var repository = DispatchProxy.Create<IGenericRepository<T>, ReversalRepositoryProxy<T>>();
            ((ReversalRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
