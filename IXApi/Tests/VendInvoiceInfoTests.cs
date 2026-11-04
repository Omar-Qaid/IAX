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
/// Pending vendor invoice entry and hold (VendInvoiceInfoTable / VendInvoiceInfoLine).
///
/// The point of this feature is that entry is separate from posting: the invoice is entered,
/// matched and held, then posted. So these tests cover what entry owns — default quantity
/// selection, the per-vendor duplicate rule, and the hold — plus the hand-off, which is that the
/// held invoice reaches the single vendor-invoice posting path with the right line, quantity and
/// price, and that the hold is released only when that path succeeds.
/// </summary>
public class VendInvoiceInfoTests
{
    [Fact]
    public async Task The_default_selection_is_the_received_but_uninvoiced_quantity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out _);

        var result = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);

        Assert.Equal(200, result.StatusCode);
        var header = Assert.Single(await db.Set<VendInvoiceInfoTable>().AsNoTracking().ToListAsync());
        Assert.Equal("INV-1", header.Num);
        Assert.Equal("V-001", header.InvoiceAccount);
        Assert.Equal("SAR", header.CurrencyCode);
        // Line 1 has 6 received and uninvoiced of 10; line 2 has nothing received, so it is not held.
        var line = Assert.Single(await db.Set<VendInvoiceInfoLine>().AsNoTracking().ToListAsync());
        Assert.Equal("ITEM-1", line.ItemId);
        Assert.Equal(6m, line.ReceiveNow);
        Assert.Equal(25m, line.PurchPrice);
        Assert.Equal(150m, line.LineAmount);
        Assert.Equal("PO-1", line.OrigPurchId);
        Assert.Equal(header.ParmId, line.ParmId);
    }

    [Fact]
    public async Task An_explicit_selection_beyond_the_received_quantity_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out _);

        var result = await service.CreateFromOrderAsync(1,
            new VendInvoiceInfoCreateDto("INV-1", Lines: [new PurchPostLineDto(1, 7m)]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("exceeds the received, uninvoiced quantity", result.Message);
        Assert.Empty(await db.Set<VendInvoiceInfoTable>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task An_order_with_nothing_received_cannot_be_invoiced()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var line = await db.Set<PurchLine>().SingleAsync(x => x.RecId == 1);
        line.RemainPurchPhysical = line.RemainPurchFinancial;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewService(db, out _);

        var result = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("no received, uninvoiced quantity", result.Message);
    }

    /// <summary>
    /// APTables.md section 8.1: check duplicates per the vendor, and do not impose uniqueness
    /// across all vendors. So the same number is refused for the same vendor and allowed for
    /// another.
    /// </summary>
    [Fact]
    public async Task A_duplicate_invoice_number_is_refused_for_the_same_vendor_only()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out _);

        Assert.Equal(200, (await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default)).StatusCode);

        var same = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);
        Assert.Equal(422, same.StatusCode);
        Assert.Contains("already held for this vendor", same.Message);

        var other = await service.CreateFromOrderAsync(2, new VendInvoiceInfoCreateDto("INV-1"), default);
        Assert.Equal(200, other.StatusCode);
        Assert.Equal(2, (await db.Set<VendInvoiceInfoTable>().AsNoTracking().ToListAsync()).Count);
    }

    [Fact]
    public async Task An_invoice_number_already_posted_for_the_vendor_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Add(new VendInvoiceJour { RecId = 1, DataAreaId = "dat", InvoiceAccount = "V-001", InvoiceId = "INV-1" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewService(db, out _);

        var result = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("already been posted", result.Message);
    }

    [Fact]
    public async Task A_vendor_blocked_for_invoicing_cannot_have_an_invoice_entered()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var vendor = await db.Set<VendTable>().SingleAsync(x => x.AccountNum == "V-001");
        vendor.Blocked = CustVendBlocked.Invoicing;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewService(db, out _);

        var result = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("blocked for invoicing", result.Message);
    }

    [Fact]
    public async Task Posting_hands_the_held_line_quantity_and_price_to_the_invoice_posting_path()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out var posting);
        var created = await service.CreateFromOrderAsync(1,
            new VendInvoiceInfoCreateDto("INV-1", Lines: [new PurchPostLineDto(1, 4m, 30m)]), default);
        var parmId = ((VendInvoiceInfoReadDto)created.Data!).ParmId;

        var result = await service.PostAsync(parmId, default);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal(1, posting.PurchTableRecId);
        Assert.Equal("INV-1", posting.Input!.InvoiceId);
        var request = Assert.Single(posting.Input.Lines);
        Assert.Equal(1, request.PurchLineRecId);
        Assert.Equal(4m, request.Qty);
        Assert.Equal(30m, request.UnitPrice);
        // The hold is released once posting has committed.
        Assert.Empty(await db.Set<VendInvoiceInfoTable>().AsNoTracking().ToListAsync());
        Assert.Empty(await db.Set<VendInvoiceInfoLine>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task A_refused_posting_leaves_the_invoice_held()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out var posting);
        posting.Result = new PurchResult(422, null, "The ledger period is not open.");
        var created = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);
        var parmId = ((VendInvoiceInfoReadDto)created.Data!).ParmId;

        var result = await service.PostAsync(parmId, default);

        Assert.Equal(422, result.StatusCode);
        Assert.Equal("The ledger period is not open.", result.Message);
        Assert.Single(await db.Set<VendInvoiceInfoTable>().AsNoTracking().ToListAsync());
        Assert.Single(await db.Set<VendInvoiceInfoLine>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task A_discarded_invoice_is_released_without_posting()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out var posting);
        var created = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);
        var parmId = ((VendInvoiceInfoReadDto)created.Data!).ParmId;

        var result = await service.DiscardAsync(parmId, default);

        Assert.Equal(200, result.StatusCode);
        Assert.Null(posting.Input);
        Assert.Empty(await db.Set<VendInvoiceInfoTable>().AsNoTracking().ToListAsync());
        Assert.Empty(await db.Set<VendInvoiceInfoLine>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task A_held_invoice_is_read_back_with_its_lines_and_total()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out _);
        var created = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1", Description: "October freight"), default);
        var parmId = ((VendInvoiceInfoReadDto)created.Data!).ParmId;

        var read = await service.GetByParmIdAsync(parmId, default);

        Assert.Equal(200, read.StatusCode);
        var dto = (VendInvoiceInfoReadDto)read.Data!;
        Assert.Equal("October freight", dto.Description);
        Assert.Equal(150m, dto.LineTotal);
        Assert.Single(dto.Lines);
    }

    [Fact]
    public async Task An_unknown_pending_invoice_is_not_found()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out _);

        Assert.Equal(404, (await service.GetByParmIdAsync("PINV-999999", default)).StatusCode);
        Assert.Equal(404, (await service.PostAsync("PINV-999999", default)).StatusCode);
        Assert.Equal(404, (await service.DiscardAsync("PINV-999999", default)).StatusCode);
    }

    [Fact]
    public async Task A_vendor_return_order_cannot_take_a_pending_invoice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var order = await db.Set<PurchTable>().SingleAsync(x => x.RecId == 1);
        order.PurchaseType = PurchaseType.ReturnItem;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewService(db, out _);

        var result = await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("INV-1"), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("return order credit note", result.Message);
    }

    [Fact]
    public async Task A_missing_order_and_an_empty_invoice_number_are_rejected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = NewService(db, out _);

        Assert.Equal(404, (await service.CreateFromOrderAsync(99, new VendInvoiceInfoCreateDto("INV-1"), default)).StatusCode);
        Assert.Equal(400, (await service.CreateFromOrderAsync(1, new VendInvoiceInfoCreateDto("  "), default)).StatusCode);
    }

    private static VendInvoiceInfoService NewService(DbContext db, out PostingSpy posting)
    {
        posting = new PostingSpy();
        var invoices = DispatchProxy.Create<IVendorInvoiceService, PostingProxy>();
        ((PostingProxy)(object)invoices).State = posting;
        return new VendInvoiceInfoService(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, PendingSequenceProxy>(), invoices);
    }

    private static async Task<PendingInvoiceContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PendingInvoiceContext(
            new DbContextOptionsBuilder<PendingInvoiceContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new PurchTable { RecId = 1, DataAreaId = "dat", PurchId = "PO-1", OrderAccount = "V-001",
                InvoiceAccount = "V-001", CurrencyCode = "SAR" },
            new PurchTable { RecId = 2, DataAreaId = "dat", PurchId = "PO-2", OrderAccount = "V-002",
                InvoiceAccount = "V-002", CurrencyCode = "SAR" },
            // 10 ordered, 6 received and uninvoiced: RemainPurchFinancial 10 less
            // RemainPurchPhysical 4 is what is awaiting an invoice.
            new PurchLine { RecId = 1, DataAreaId = "dat", PurchId = "PO-1", LineNumber = 1, ItemId = "ITEM-1",
                InventTransId = "LOT-1", InventDimId = "DIM-1", PurchUnit = "ea", PurchQty = 10, QtyOrdered = 10,
                PurchPrice = 25m, PriceUnit = 1m, RemainPurchFinancial = 10m, RemainPurchPhysical = 4m },
            // Nothing received, so the default selection must skip it.
            new PurchLine { RecId = 2, DataAreaId = "dat", PurchId = "PO-1", LineNumber = 2, ItemId = "ITEM-2",
                InventTransId = "LOT-2", InventDimId = "DIM-1", PurchUnit = "ea", PurchQty = 5, QtyOrdered = 5,
                PurchPrice = 10m, PriceUnit = 1m, RemainPurchFinancial = 5m, RemainPurchPhysical = 5m },
            new PurchLine { RecId = 3, DataAreaId = "dat", PurchId = "PO-2", LineNumber = 1, ItemId = "ITEM-1",
                InventTransId = "LOT-3", InventDimId = "DIM-1", PurchUnit = "ea", PurchQty = 2, QtyOrdered = 2,
                PurchPrice = 25m, PriceUnit = 1m, RemainPurchFinancial = 2m, RemainPurchPhysical = 0m },
            new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001" },
            new VendTable { RecId = 2, DataAreaId = "dat", AccountNum = "V-002" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class PendingInvoiceContext(DbContextOptions<PendingInvoiceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(PurchTable), typeof(PurchLine), typeof(VendTable),
                typeof(VendInvoiceJour), typeof(VendInvoiceInfoTable), typeof(VendInvoiceInfoLine), typeof(VendInvoiceInfoSubLine),
                typeof(SourceDocumentHeader), typeof(SourceDocumentLine), typeof(VendPackingSlipTrans), typeof(VendInvoicePackingSlipQuantityMatch)];

            // VendTable reaches the party and address graph, so everything not named here is
            // ignored outright rather than discovered one failure at a time.
            var included = new HashSet<Type>(types);
            foreach (var type in typeof(PurchTable).Assembly.GetTypes()
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

    /// <summary>What the posting path was handed, and what it should answer.</summary>
    private sealed class PostingSpy
    {
        public long? PurchTableRecId { get; set; }
        public VendorInvoicePostDto? Input { get; set; }
        public PurchResult Result { get; set; } = new(200, new { posted = true }, "Vendor invoice posted.");
    }

    /// <summary>
    /// Stands in for the single vendor-invoice posting path. IBaseService is wide, and only
    /// PostAsync is of interest, so the rest is left unimplemented on purpose: a call to anything
    /// else means the pending invoice reached past the one method it is supposed to use.
    /// </summary>
    public class PostingProxy : DispatchProxy
    {
        internal object State { get; set; } = null!;

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name != nameof(IVendorInvoiceService.PostAsync))
                throw new NotSupportedException(method?.Name);
            var spy = (PostingSpy)State;
            spy.PurchTableRecId = (long)args![0]!;
            spy.Input = (VendorInvoicePostDto)args[1]!;
            return Task.FromResult(spy.Result);
        }
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }

    public class PendingSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            method?.Name == nameof(ISysNumberSequenceService.NextAsync)
                ? Task.FromResult(new NextSequenceResultDto
                    { EntityName = "VendInvoiceInfoTable", Value = ++_n, Code = $"PINV-{_n:D6}" })
                : throw new NotSupportedException(method?.Name);
    }

    public class PendingRepositoryProxy<T> : DispatchProxy where T : class
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
            // BaseService resolves this in its constructor. The service itself works through
            // Context.Set<T>(), so the proxy only needs to exist.
            var repository = DispatchProxy.Create<IGenericRepository<T>, PendingRepositoryProxy<T>>();
            ((PendingRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
