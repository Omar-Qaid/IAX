using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Application.NumberSequences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Taking a cash discount at customer payment settlement.
///
/// The discount is earned only when the payment arrives on or before the discount date and covers
/// the whole invoice less the discount — a part payment earns nothing. When it is earned, the
/// invoice is cleared in full for less cash and the difference posts to the account configured on
/// the discount code, never to a default.
///
/// The voucher becomes Dr Bank for the cash, Dr cash discount for the discount, Cr customer for
/// both: the customer's balance falls by what was cleared, not only by what was banked.
/// </summary>
public class CashDiscountSettlementTests
{
    private const decimal Invoice = 1000m;

    [Fact]
    public async Task A_payment_in_time_clears_the_invoice_for_less_cash_and_posts_the_discount()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        // 2% of 1000 is 20, so 980 on or before the discount date clears the whole invoice.
        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", 980m, new DateTime(2026, 1, 10)), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(Invoice, invoice.SettleAmountCur);
        Assert.Equal(CustSettlementStatus.Settled, invoice.Settlement);
        Assert.Empty(await db.Set<CustTransOpen>().AsNoTracking().Where(x => x.RefRecId == 10).ToListAsync());

        var settlement = await db.Set<CustSettlement>().AsNoTracking()
            .SingleAsync(x => x.TransRecId == 10);
        Assert.Equal(Invoice, settlement.SettleAmountCur);
        Assert.Equal(20m, settlement.UtilizedCashDisc);
        Assert.Equal(5, settlement.CashDiscountLedgerDimension);
    }

    [Fact]
    public async Task The_voucher_balances_with_the_discount_included()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", 980m, new DateTime(2026, 1, 10)), default);

        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(3, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(980m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
        Assert.Equal(20m, entries.Single(x => x.PostingType == LedgerPostingType.CashDiscount).AccountingCurrencyAmount);
        // The customer is credited for everything cleared, not only the cash banked.
        Assert.Equal(-1000m, entries.Single(x => x.PostingType == LedgerPostingType.CustBalance).AccountingCurrencyAmount);
        var date = new DateTime(2026, 1, 10);
        var statement = await new CustTransService(new TestUnitOfWork(db), new TestUser())
            .GetStatementAsync("C-001", date, date, default);
        Assert.Equal(1000m, statement.OpeningBalanceMst);
        Assert.Equal(0m, statement.ClosingBalanceMst);
        var receipt = Assert.Single(statement.Lines);
        Assert.Equal(-980m, receipt.AmountCur);
        Assert.Equal(-1000m, receipt.AmountMst);
    }

    [Fact]
    public async Task A_payment_after_the_discount_date_earns_nothing_and_settles_only_the_cash()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        // The discount date is 2026-01-11; this is a day late.
        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", 980m, new DateTime(2026, 1, 12)), default);

        Assert.Equal(200, result.StatusCode);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(980m, invoice.SettleAmountCur);
        // 20 is still owed, so the invoice stays open.
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 10);
        Assert.Equal(20m, open.AmountCur);
        var settlement = await db.Set<CustSettlement>().AsNoTracking().SingleAsync(x => x.TransRecId == 10);
        Assert.Equal(0m, settlement.UtilizedCashDisc);
        // Two entries only: no discount account is touched.
        Assert.Equal(2, (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync()).Count);
    }

    [Fact]
    public async Task A_part_payment_that_cannot_clear_the_invoice_earns_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        // In time, but 500 cannot clear 1000 less 20.
        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", 500m, new DateTime(2026, 1, 10)), default);

        Assert.Equal(200, result.StatusCode);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(500m, invoice.SettleAmountCur);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 10);
        Assert.Equal(500m, open.AmountCur);
        var settlement = await db.Set<CustSettlement>().AsNoTracking().SingleAsync(x => x.TransRecId == 10);
        Assert.Equal(0m, settlement.UtilizedCashDisc);
    }

    [Fact]
    public async Task An_invoice_with_no_discount_code_settles_exactly_as_before()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var invoice = await db.Set<CustTrans>().SingleAsync(x => x.RecId == 10);
        invoice.CashDiscCode = string.Empty;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", 1000m, new DateTime(2026, 1, 10)), default);

        Assert.Equal(200, result.StatusCode);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(-1000m, entries.Single(x => x.PostingType == LedgerPostingType.CustBalance).AccountingCurrencyAmount);
    }

    /// <summary>
    /// A discount cannot be taken without an account to post it to, and no default is assumed.
    /// </summary>
    [Fact]
    public async Task A_discount_code_with_no_ledger_account_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var discount = await db.Set<CashDisc>().SingleAsync();
        discount.CashDiscLedgerDimension = 0;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", 980m, new DateTime(2026, 1, 10)), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("no ledger account configured", result.Message);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task An_unknown_discount_code_is_treated_as_no_discount()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Remove(await db.Set<CashDisc>().SingleAsync());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", 1000m, new DateTime(2026, 1, 10)), default);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal(2, (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync()).Count);
    }

    // ---------- Accounts payable ----------

    [Fact]
    public async Task A_vendor_payment_in_time_clears_the_invoice_for_less_cash_and_posts_the_discount()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var result = await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 980m, "req-1", new DateTime(2026, 1, 10)), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 20);
        // The payable path subtracts on the invoice side, so a cleared credit reads negative.
        Assert.Equal(-Invoice, invoice.SettleAmountCur);
        Assert.Empty(await db.Set<VendTransOpen>().AsNoTracking().Where(x => x.RefRecId == 20).ToListAsync());
        var settlement = await db.Set<VendSettlement>().AsNoTracking().SingleAsync();
        Assert.Equal(Invoice, settlement.AmountCur);
        Assert.Equal(20m, settlement.UtilizedCashDisc);
    }

    [Fact]
    public async Task The_vendor_voucher_balances_with_the_discount_included()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 980m, "req-1", new DateTime(2026, 1, 10)), default);

        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(3, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        // Dr the vendor for everything cleared, Cr Bank for the cash, Cr the discount account.
        Assert.Equal(1000m, entries.Single(x => x.PostingType == LedgerPostingType.VendBalance).AccountingCurrencyAmount);
        Assert.Equal(-980m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
        Assert.Equal(-20m, entries.Single(x => x.PostingType == LedgerPostingType.CashDiscount).AccountingCurrencyAmount);
    }

    [Fact]
    public async Task A_late_vendor_payment_earns_nothing_and_leaves_the_invoice_open()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        var result = await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 980m, "req-1", new DateTime(2026, 1, 12)), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var open = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 20);
        Assert.Equal(-20m, open.AmountCur);
        Assert.Equal(0m, (await db.Set<VendSettlement>().AsNoTracking().SingleAsync()).UtilizedCashDisc);
        Assert.Equal(2, (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync()).Count);
    }

    /// <summary>
    /// A payable settlement that took a discount posted to the ledger, and the restore path writes
    /// no counter-voucher, so it refuses rather than stranding that posting.
    /// </summary>
    [Fact]
    public async Task A_vendor_settlement_that_took_a_discount_cannot_be_reversed()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 980m, "req-1", new DateTime(2026, 1, 10)), default);
        db.ChangeTracker.Clear();
        var voucher = (await db.Set<VendSettlement>().AsNoTracking().SingleAsync()).Voucher;

        var reversal = await new VendSettlementReversalService(new TestUnitOfWork(db), new TestCompany())
            .ReverseAsync(voucher, new DateTime(2026, 1, 20), default);

        Assert.Equal(422, reversal.StatusCode);
        Assert.Contains("carries a cash discount", reversal.Message);
    }

    // ---------- Vendor refund ----------

    /// <summary>
    /// A vendor credit note leaves the vendor owing us money: an open vendor <b>debit</b>. When the
    /// vendor sends the cash back, the bank is debited, the payable is credited to bring it back to
    /// zero, and the two vendor rows settle against each other so nothing is left to recover.
    /// </summary>
    [Fact]
    public async Task A_vendor_refund_banks_the_money_and_settles_the_open_debit()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorCredit(db, 300m);

        var result = await NewVendService(db).RefundAsync(
            new VendRefundPostDto("V-001", "BANK-1", 21, "refund-1", new DateTime(2026, 1, 10)), default);

        Assert.True(result.StatusCode == 200, result.Message);
        // The credit is fully recovered, so both its open row and the refund's are gone.
        Assert.Empty(await db.Set<VendTransOpen>().AsNoTracking().Where(x => x.RefRecId != 20).ToListAsync());
        var credit = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 21);
        Assert.Equal(300m, credit.SettleAmountCur);
        Assert.Equal(new DateTime(2026, 1, 10), credit.Closed);
        // The refund itself is the negative side and is fully settled too.
        var refund = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId != 20 && x.RecId != 21);
        Assert.Equal(-300m, refund.AmountCur);
        Assert.Equal(-300m, refund.SettleAmountCur);
        // Sign and record-id conventions match VendSettleOpenTransactionsService so the existing
        // restore reverses this row unchanged: positive magnitude, refund as the invoice side.
        var settlement = await db.Set<VendSettlement>().AsNoTracking().SingleAsync();
        Assert.Equal(300m, settlement.AmountCur);
        Assert.Equal(refund.RecId, settlement.InvoiceTransRecId);
        Assert.Equal(21, settlement.PaymentTransRecId);
        Assert.Equal(NoYes.Yes, settlement.CanBeReversed);
        // The voucher balances: bank debited, payable credited back to zero.
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(300m, entries.Where(x => x.AccountingCurrencyAmount > 0m).Sum(x => x.AccountingCurrencyAmount));
    }

    /// <summary>
    /// The request ID is the replay key, as it is for a vendor payment: sending the same refund
    /// twice returns the first one instead of taking the money twice.
    /// </summary>
    [Fact]
    public async Task Repeating_a_vendor_refund_request_does_not_refund_twice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorCredit(db, 300m);
        var input = new VendRefundPostDto("V-001", "BANK-1", 21, "refund-1", new DateTime(2026, 1, 10));

        Assert.Equal(200, (await NewVendService(db).RefundAsync(input, default)).StatusCode);
        var again = await NewVendService(db).RefundAsync(input, default);

        Assert.Equal(200, again.StatusCode);
        Assert.Contains("already posted", again.Message);
        Assert.Single(await db.Set<VendSettlement>().AsNoTracking().ToListAsync());
        Assert.Equal(2, (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync()).Count);
    }

    /// <summary>
    /// An invoice is a vendor credit and is paid, not refunded, so selecting one is refused rather
    /// than posting a payment under the refund's name.
    /// </summary>
    [Fact]
    public async Task Refunding_an_invoice_instead_of_an_open_debit_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        // RecId 20 is the seeded vendor invoice: an open credit, not a debit.
        var refused = await NewVendService(db).RefundAsync(
            new VendRefundPostDto("V-001", "BANK-1", 20, "refund-1", new DateTime(2026, 1, 10)), default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("Select an open vendor debit", refused.Message);
        Assert.Empty(await db.Set<VendSettlement>().AsNoTracking().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
    }

    /// <summary>
    /// A vendor credit note the vendor still owes us, added per test so the shared fixture's
    /// payment scenarios keep seeing only the invoice.
    /// </summary>
    private static void SeedVendorCredit(DbContext db, decimal amount)
    {
        db.AddRange(
            new VendTrans
            {
                RecId = 21, DataAreaId = "dat", AccountNum = "V-001", Invoice = "VCN-1", Voucher = "VOU-VCN",
                TransDate = new DateTime(2026, 1, 2), DueDate = new DateTime(2026, 1, 2), CurrencyCode = "SAR",
                AmountCur = amount, AmountMst = amount, TransType = LedgerTransType.Purch
            },
            new VendTransOpen
            {
                RecId = 21, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 21,
                TransDate = new DateTime(2026, 1, 2), DueDate = new DateTime(2026, 1, 2),
                AmountCur = amount, AmountMst = amount
            });
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }

    // ---------- Fixture ----------

    private static VendPaymentService NewVendService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, CashDiscountSequenceProxy>());

    private static CustPaymentService NewService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, CashDiscountSequenceProxy>());

    private static async Task<CashDiscountContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new CashDiscountContext(
            new DbContextOptionsBuilder<CashDiscountContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new Ledger { RecId = 1, DataAreaId = "dat", ChartOfAccounts = 10, FiscalCalendar = 1, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) },
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new MainAccount { RecId = 1, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "110100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "130100" },
            new MainAccount { RecId = 3, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "610300" },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "110100", DisplayValue = "110100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "130100", DisplayValue = "130100" },
            // The cash discount account.
            new DimensionAttributeValueCombination { RecId = 5, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "610300", DisplayValue = "610300" },
            new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM", CurrencyCode = "SAR" },
            new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 },
            new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 1,
                CurrencyCode = "SAR", BankAccountStatus = BankAccountStatus.Active },
            // 2% within 10 days of 2026-01-01, so the discount date is 2026-01-11.
            new CashDisc { RecId = 1, DataAreaId = "dat", CashDiscCode = "2PCT10", Description = "2% 10 days",
                NumOfDays = 10, DiscPct = 2m, CashDiscLedgerDimension = 5 },
            new CustTrans
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "C-001", Invoice = "INV-1", Voucher = "VOU-INV",
                TransDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 1, 31), CurrencyCode = "SAR",
                AmountCur = Invoice, AmountMst = Invoice, ReportingCurrencyAmount = Invoice,
                TransType = LedgerTransType.Sales,
                CashDiscCode = "2PCT10", CashDiscBaseDate = new DateTime(2026, 1, 1)
            },
            new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", Currency = "SAR" },
            // The vendor disbursement journal a refund posts into, which LedgerJournalSetupCheck
            // requires to resolve uniquely and to match the header type.
            new LedgerJournalName { RecId = 1, DataAreaId = "dat", JournalName = "VendPaym",
                JournalType = LedgerJournalType.VendDisbursement },
            new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 },
            new VendTrans
            {
                RecId = 20, DataAreaId = "dat", AccountNum = "V-001", Invoice = "VINV-1", Voucher = "VOU-VINV",
                TransDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 1, 31), CurrencyCode = "SAR",
                AmountCur = -Invoice, AmountMst = -Invoice, TransType = LedgerTransType.Purch,
                CashDiscCode = "2PCT10", CashDiscBaseDate = new DateTime(2026, 1, 1)
            },
            new VendTransOpen
            {
                RecId = 20, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 20,
                TransDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 1, 31),
                AmountCur = -Invoice, AmountMst = -Invoice
            },
            new CustTransOpen
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 10,
                TransDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 1, 31),
                AmountCur = Invoice, AmountMst = Invoice, ReportingCurrencyAmount = Invoice,
                CashDiscDate = new DateTime(2026, 1, 11), PossibleCashDisc = 20m,
                CashDiscountLedgerDimension = 5
            });
        // Posting requires an active structure assigned to the ledger.
        TestAccountStructures.AssignPermissive(db, "dat", 1);
        db.SaveChanges();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class CashDiscountContext(DbContextOptions<CashDiscountContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement), typeof(CustTable),
                typeof(CustParameters), typeof(CustLedgerAccounts), typeof(CashDisc),
                typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(VendInvoiceJour), typeof(VendTable),
                typeof(VendLedgerAccounts),
                typeof(VendParameters),
                typeof(LedgerJournalTable), typeof(LedgerJournalTrans), typeof(LedgerJournalName),
                typeof(BankAccountTable), typeof(BankAccountTrans),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(MainAccountLegalEntity), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry),
                typeof(SubledgerVoucherGeneralJournalEntry),
                // The wider graph LedgerJournalPosting pulls in, matching LedgerJournalPostingTests:
                // the vendor refund posts through that path rather than hand-rolling its subledger.
                typeof(SysNumberSequence), typeof(CompanyInfo),
                typeof(IAX.IXApi.Modules.Finance.Entities.LedgerParameters),
                typeof(ExchangeRateType), typeof(ExchangeRateCurrencyPair), typeof(ExchangeRate),
                typeof(Currency),
                .. TestAccountStructures.Types];

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

    public class CashDiscountSequenceProxy : DispatchProxy
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

    public class CashDiscountRepositoryProxy<T> : DispatchProxy where T : class
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
            var repository = DispatchProxy.Create<IGenericRepository<T>, CashDiscountRepositoryProxy<T>>();
            ((CashDiscountRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
