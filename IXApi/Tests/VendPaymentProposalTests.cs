using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// D365's vendor payment proposal: selecting the open invoices to pay by due date, vendor,
/// currency and method of payment, and stating what paying them would cost.
///
/// <para>
/// A proposal is a selection, not a document. These tests assert that nothing is posted, that the
/// filters behave as D365's do, and that the cash discount it promises is the one the payment
/// service would actually take - the proposal reuses that service's own resolver rather than
/// recomputing the rule.
/// </para>
/// </summary>
public class VendPaymentProposalTests
{
    private static readonly DateTime Invoiced = new(2026, 10, 1);

    private static VendPaymentProposalDto Proposal(PurchResult result)
    {
        Assert.Equal(200, result.StatusCode);
        return Assert.IsType<VendPaymentProposalDto>(result.Data);
    }

    [Fact]
    public async Task The_proposal_lists_open_invoices_oldest_due_first_and_posts_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, "V-001");
        AddInvoice(db, 1, "V-001", "INV-LATE", -400m, due: Invoiced.AddDays(60));
        AddInvoice(db, 2, "V-001", "INV-EARLY", -600m, due: Invoiced.AddDays(30));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var proposal = Proposal(await Service(db).ProposeAsync(new(ProposalDate: Invoiced.AddDays(31)), default));

        Assert.Equal(2, proposal.Lines.Count);
        Assert.Equal("INV-EARLY", proposal.Lines[0].Invoice);
        Assert.Equal("INV-LATE", proposal.Lines[1].Invoice);
        // Open payable amounts are credits; the proposal states them as positive magnitudes.
        Assert.Equal(600m, proposal.Lines[0].OpenAmountCur);
        Assert.Equal(1000m, proposal.TotalsByCurrency["SAR"]);
        Assert.False(proposal.Posted);
        // A proposal writes nothing at all.
        Assert.Empty(await db.Set<VendSettlement>().AsNoTracking().ToListAsync());
        Assert.Equal(2, await db.Set<VendTransOpen>().AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task A_part_settled_invoice_is_proposed_for_only_what_is_still_open()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, "V-001");
        AddInvoice(db, 1, "V-001", "INV-1", -1000m, due: Invoiced.AddDays(30));
        // 400 already paid: the open row holds the remainder, which is what must be proposed.
        db.Set<VendTransOpen>().Local.Single().AmountCur = -600m;
        db.Set<VendTrans>().Local.Single().SettleAmountCur = -400m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var proposal = Proposal(await Service(db).ProposeAsync(new(ProposalDate: Invoiced.AddDays(31)), default));

        var line = Assert.Single(proposal.Lines);
        Assert.Equal(600m, line.OpenAmountCur);
        Assert.Equal(600m, line.AmountToPay);
    }

    [Fact]
    public async Task A_vendor_blocked_for_payment_is_excluded_and_named_rather_than_silently_dropped()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, "V-001");
        SeedVendor(db, "V-BLOCKED", blocked: CustVendBlocked.Payment);
        SeedVendor(db, "V-ALL", blocked: CustVendBlocked.All);
        SeedVendor(db, "V-INVOICING", blocked: CustVendBlocked.Invoicing);
        AddInvoice(db, 1, "V-001", "INV-1", -100m, due: Invoiced.AddDays(30));
        AddInvoice(db, 2, "V-BLOCKED", "INV-2", -200m, due: Invoiced.AddDays(30));
        AddInvoice(db, 3, "V-ALL", "INV-3", -300m, due: Invoiced.AddDays(30));
        AddInvoice(db, 4, "V-INVOICING", "INV-4", -400m, due: Invoiced.AddDays(30));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var proposal = Proposal(await Service(db).ProposeAsync(new(ProposalDate: Invoiced.AddDays(31)), default));

        // Blocked for payment or for everything is excluded; blocked for invoicing only is not,
        // because that block is about receiving invoices, not about paying them.
        Assert.Equal(["INV-1", "INV-4"], proposal.Lines.Select(x => x.Invoice).Order());
        Assert.Equal(["V-ALL", "V-BLOCKED"], proposal.ExcludedBlockedVendors.Order());
    }

    [Fact]
    public async Task The_due_date_vendor_currency_and_method_filters_each_narrow_the_selection()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, "V-001", paymMode: "CHECK");
        SeedVendor(db, "V-002", paymMode: "WIRE");
        AddInvoice(db, 1, "V-001", "SAR-SOON", -100m, due: Invoiced.AddDays(10));
        AddInvoice(db, 2, "V-001", "SAR-LATER", -200m, due: Invoiced.AddDays(90));
        AddInvoice(db, 3, "V-001", "USD-SOON", -300m, due: Invoiced.AddDays(10), currency: "USD");
        AddInvoice(db, 4, "V-002", "OTHER-VENDOR", -400m, due: Invoiced.AddDays(10));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = Service(db);

        var byDue = Proposal(await service.ProposeAsync(new(DueBy: Invoiced.AddDays(30)), default));
        Assert.Equal(["OTHER-VENDOR", "SAR-SOON", "USD-SOON"], byDue.Lines.Select(x => x.Invoice).Order());

        var byVendor = Proposal(await service.ProposeAsync(new(VendAccount: "V-002"), default));
        Assert.Equal(["OTHER-VENDOR"], byVendor.Lines.Select(x => x.Invoice));

        var byCurrency = Proposal(await service.ProposeAsync(new(CurrencyCode: "USD"), default));
        Assert.Equal(["USD-SOON"], byCurrency.Lines.Select(x => x.Invoice));

        var byMethod = Proposal(await service.ProposeAsync(new(PaymMode: "WIRE"), default));
        Assert.Equal(["OTHER-VENDOR"], byMethod.Lines.Select(x => x.Invoice));

        // Filters combine rather than replace each other.
        var combined = Proposal(await service.ProposeAsync(
            new(DueBy: Invoiced.AddDays(30), VendAccount: "V-001", CurrencyCode: "SAR"), default));
        Assert.Equal(["SAR-SOON"], combined.Lines.Select(x => x.Invoice));
    }

    [Fact]
    public async Task The_proposed_cash_discount_is_the_one_still_available_on_the_proposal_date()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, "V-001");
        db.Add(new CashDisc { RecId = 1, DataAreaId = "dat", CashDiscCode = "2%10", NumOfDays = 10, DiscPct = 2m });
        AddInvoice(db, 1, "V-001", "INV-1", -1000m, due: Invoiced.AddDays(30), cashDiscCode: "2%10");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = Service(db);

        // Day 5: inside the ten-day window, so 2% is available and the cash needed is 980.
        var inTime = Proposal(await service.ProposeAsync(new(ProposalDate: Invoiced.AddDays(5)), default));
        Assert.Equal(20m, inTime.Lines[0].CashDiscountAmount);
        Assert.Equal(980m, inTime.Lines[0].AmountToPay);
        Assert.Equal(20m, inTime.TotalCashDiscount);

        // Day 20: the window has passed, so the full 1,000 is owed.
        var tooLate = Proposal(await service.ProposeAsync(new(ProposalDate: Invoiced.AddDays(20)), default));
        Assert.Equal(0m, tooLate.Lines[0].CashDiscountAmount);
        Assert.Equal(1000m, tooLate.Lines[0].AmountToPay);
    }

    [Fact]
    public async Task A_debit_open_row_is_not_proposed_for_payment()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, "V-001");
        // A credit note or an unapplied payment leaves a debit open row. It is settled against an
        // invoice, never paid out again, so the proposal must not offer it.
        AddInvoice(db, 1, "V-001", "VCN-1", 500m, due: Invoiced.AddDays(30));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var proposal = Proposal(await Service(db).ProposeAsync(new(ProposalDate: Invoiced.AddDays(31)), default));

        Assert.Empty(proposal.Lines);
        Assert.Equal("No open vendor invoices match the payment proposal criteria.",
            (await Service(db).ProposeAsync(new(), default)).Message);
    }

    [Fact]
    public async Task An_unknown_vendor_or_method_of_payment_is_refused_by_name()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendor(db, "V-001");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = Service(db);

        var vendor = await service.ProposeAsync(new(VendAccount: "V-404"), default);
        Assert.Equal(404, vendor.StatusCode);
        Assert.Equal("Vendor 'V-404' was not found in the selected company.", vendor.Message);

        var method = await service.ProposeAsync(new(PaymMode: "CASHIERS-CHEQUE"), default);
        Assert.Equal(404, method.StatusCode);
        Assert.Equal("Method of payment 'CASHIERS-CHEQUE' was not found in the selected company.", method.Message);
    }

    // ---------- fixture ----------

    private static VendPaymentProposalService Service(ProposalContext db) =>
        new(new ProposalUnitOfWork(db), new ProposalCompany());

    private static void SeedVendor(ProposalContext db, string account,
        CustVendBlocked blocked = CustVendBlocked.No, string paymMode = "CHECK")
    {
        if (!db.Set<VendPaymModeTable>().Local.Any(x => x.PaymMode == paymMode))
            db.Add(new VendPaymModeTable
            {
                RecId = db.Set<VendPaymModeTable>().Local.Count + 1, DataAreaId = "dat", PaymMode = paymMode
            });
        db.Add(new VendTable
        {
            RecId = db.Set<VendTable>().Local.Count + 1, DataAreaId = "dat", AccountNum = account,
            Currency = "SAR", PaymMode = paymMode, Blocked = blocked
        });
    }

    private static void AddInvoice(ProposalContext db, long recId, string account, string invoice,
        decimal amount, DateTime due, string currency = "SAR", string cashDiscCode = "")
    {
        db.Add(new VendTrans
        {
            RecId = recId, DataAreaId = "dat", AccountNum = account, Invoice = invoice,
            Voucher = "V" + recId, TransDate = Invoiced, DueDate = due, CurrencyCode = currency,
            AmountCur = amount, AmountMst = amount, CashDiscCode = cashDiscCode,
            TransType = LedgerTransType.Purch
        });
        db.Add(new VendTransOpen
        {
            RecId = recId, DataAreaId = "dat", AccountNum = account, RefRecId = recId,
            TransDate = Invoiced, DueDate = due, AmountCur = amount, AmountMst = amount
        });
    }

    private static async Task<ProposalContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new ProposalContext(new DbContextOptionsBuilder<ProposalContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class ProposalContext(DbContextOptions<ProposalContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(VendTrans), typeof(VendTransOpen), typeof(VendTable),
                typeof(VendPaymModeTable), typeof(CashDisc), typeof(VendSettlement)];

            // VendTable reaches the party and address graph, so everything not named here is
            // ignored outright rather than discovered one failure at a time.
            var included = new HashSet<Type>(types);
            foreach (var type in typeof(VendTrans).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(IAX.IXApi.Shared.Domain.Entities.Entity<long>).IsAssignableFrom(t)
                    && !included.Contains(t)))
                builder.Ignore(type);

            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.ToTable(type.Name);
                foreach (var property in type.GetProperties()
                    .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string)
                        && !included.Contains(p.PropertyType)))
                    entity.Ignore(property.Name);
            }
            base.OnModelCreating(builder);
        }
    }

    private sealed class ProposalUnitOfWork(DbContext db) : IUnitOfWork
    {
        public DbContext Context => db;
        public IGenericRepository<T> Repository<T>() where T : class =>
            throw new NotSupportedException("The proposal only reads through Context.");
        public Task<int> CompleteAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
    }

    private sealed class ProposalCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }
}
