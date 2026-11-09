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
/// Payment reversal for both subledgers — the other half of the settlement-reversal gap.
///
/// A reversal is itself a posted document, as LedgerJournalReversalService established for the
/// general journal: a new LedgerJournalTable whose OriginalJournalNum points at the reversed
/// journal, with its own voucher and accounting date. Nothing posted is edited or deleted, a
/// payment is reversed once, and a reversal cannot be reversed.
///
/// Reversing a payment has to unwind that payment's settlements first, or the counter-voucher
/// would leave the subledger disagreeing with the ledger. That unwind runs inside the reversal's
/// own transaction through the shared restore helper, so the whole thing is atomic.
/// </summary>
public class PaymentReversalTests
{
    private static readonly DateTime Posted = new(2026, 1, 10);
    private static readonly DateTime Reversed = new(2026, 1, 20);

    // ---------- Accounts receivable ----------

    [Fact]
    public async Task An_unallocated_customer_payment_cannot_be_reversed_before_its_posting_date()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = NewCustReversal(db);
        var refused = await service.ReverseAsync("LJT-PAY", Posted.AddDays(-1), default);
        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("precedes", refused.Message);
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync());
        Assert.Single(await db.Set<BankAccountTrans>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Empty(await db.Set<CustSettlement>().ToListAsync());
        Assert.Equal(-400m, (await db.Set<CustTransOpen>().SingleAsync()).AmountCur);
        Assert.Equal(200, (await service.ReverseAsync("LJT-PAY", Posted, default)).StatusCode);
    }

    [Theory]
    [InlineData(false, "130100")]
    [InlineData(true, "210100")]
    public async Task Reversing_a_customer_payment_posts_the_counter_voucher(bool prepayment, string expectedAccount)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m);
        if (prepayment)
        {
            db.Set<CustTrans>().Local.Single().Prepayment = NoYes.Yes;
            var line = db.Set<LedgerJournalTrans>().Local.Single();
            line.Prepayment = NoYes.Yes;
            line.LedgerDimension = 3;
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustReversal(db).ReverseAsync("LJT-PAY", Reversed, default);

        Assert.Equal(200, result.StatusCode);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        // Mirror of Dr Bank / Cr Customer: the customer is debited and the bank credited.
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        var customerEntry = entries.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        Assert.Equal(expectedAccount, customerEntry.LedgerAccount);
        Assert.Equal(400m, customerEntry.AccountingCurrencyAmount);
        Assert.Equal(-400m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
        // The voucher is associated, as every posting in this module must be.
        Assert.Single(await db.Set<SubledgerVoucherGeneralJournalEntry>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task The_reversal_is_its_own_journal_pointing_at_the_one_it_reverses()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await NewCustReversal(db).ReverseAsync("LJT-PAY", Reversed, default);

        var reversal = await db.Set<LedgerJournalTable>().AsNoTracking()
            .SingleAsync(x => x.OriginalJournalNum == "LJT-PAY");
        Assert.Equal(NoYes.Yes, reversal.Posted);
        Assert.Equal(LedgerJournalType.CustPayment, reversal.JournalType);
        // The original is untouched: nothing posted is edited.
        var original = await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync(x => x.JournalNum == "LJT-PAY");
        Assert.Equal(string.Empty, original.OriginalJournalNum);
        Assert.Equal(NoYes.Yes, original.Posted);
        // The reversal's accounting date is its own, not the payment's.
        var line = await db.Set<LedgerJournalTrans>().AsNoTracking()
            .SingleAsync(x => x.JournalNum == reversal.JournalNum);
        Assert.Equal(Reversed, line.TransDate);
        Assert.Equal(400m, line.AmountCurDebit);
    }

    [Fact]
    public async Task Reversing_a_payment_reopens_the_invoice_it_had_settled()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // A 1000 invoice settled 400 by the payment, leaving 600 open.
        SeedCustomerPayment(db, amount: 400m, withSettledInvoice: true, invoiceAmount: 1000m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewCustReversal(db).ReverseAsync("LJT-PAY", Reversed, default);

        Assert.Equal(200, result.StatusCode);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(0m, invoice.SettleAmountCur);
        Assert.Equal(CustSettlementStatus.None, invoice.Settlement);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 10);
        Assert.Equal(1000m, open.AmountCur);
    }

    [Fact]
    public async Task The_payment_and_its_reversal_are_left_settled_against_each_other()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await NewCustReversal(db).ReverseAsync("LJT-PAY", Reversed, default);

        var payment = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 20);
        var reversal = await db.Set<CustTrans>().AsNoTracking()
            .SingleAsync(x => x.RecId != 20 && x.TransType == LedgerTransType.Payment);
        Assert.Equal(400m, reversal.AmountCur);
        // Neither is left open.
        Assert.Equal(0m, payment.AmountCur - payment.SettleAmountCur);
        Assert.Equal(0m, reversal.AmountCur - reversal.SettleAmountCur);
        Assert.Empty(await db.Set<CustTransOpen>().AsNoTracking().Where(x => x.RefRecId == 20).ToListAsync());
    }

    [Fact]
    public async Task A_customer_payment_cannot_be_reversed_twice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewCustReversal(db);

        Assert.Equal(200, (await service.ReverseAsync("LJT-PAY", Reversed, default)).StatusCode);
        var second = await service.ReverseAsync("LJT-PAY", Reversed, default);

        Assert.Equal(422, second.StatusCode);
        Assert.Contains("already been reversed", second.Message);
        // Still only one counter-voucher.
        Assert.Equal(2, (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync()).Count);
    }

    [Fact]
    public async Task A_reversal_journal_cannot_itself_be_reversed()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewCustReversal(db);
        await service.ReverseAsync("LJT-PAY", Reversed, default);
        var reversalJournal = (await db.Set<LedgerJournalTable>().AsNoTracking()
            .SingleAsync(x => x.OriginalJournalNum == "LJT-PAY")).JournalNum;

        var result = await service.ReverseAsync(reversalJournal, Reversed, default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("itself a reversal", result.Message);
    }

    [Fact]
    public async Task A_reversal_into_a_closed_period_is_refused_and_nothing_is_written()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m, withSettledInvoice: true, invoiceAmount: 1000m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // February has no open ledger period in the fixture.
        var result = await NewCustReversal(db).ReverseAsync("LJT-PAY", new DateTime(2026, 2, 10), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
        // The settlement unwind rolled back with it: the invoice is still settled.
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(400m, invoice.SettleAmountCur);
    }

    [Fact]
    public async Task An_unknown_journal_and_a_non_payment_journal_are_rejected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedCustomerPayment(db, amount: 400m);
        db.Add(new LedgerJournalTable
        {
            RecId = 90, DataAreaId = "dat", JournalNum = "LJT-GEN", JournalName = "GEN",
            JournalType = LedgerJournalType.Daily, CurrencyCode = "SAR", Posted = NoYes.Yes
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewCustReversal(db);

        Assert.Equal(404, (await service.ReverseAsync("NOPE", Reversed, default)).StatusCode);
        Assert.Equal(400, (await service.ReverseAsync("  ", Reversed, default)).StatusCode);
        var wrongType = await service.ReverseAsync("LJT-GEN", Reversed, default);
        Assert.Equal(422, wrongType.StatusCode);
        Assert.Contains("not a customer payment journal", wrongType.Message);
    }

    // ---------- Accounts payable ----------

    [Fact]
    public async Task Reversing_a_vendor_payment_posts_the_counter_voucher()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendReversal(db).ReverseAsync("LJT-VPAY", Reversed, default);

        Assert.Equal(200, result.StatusCode);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        // Mirror of Dr Vendor / Cr Bank: the vendor is credited and the bank debited.
        Assert.Equal(-400m, entries.Single(x => x.PostingType == LedgerPostingType.VendBalance).AccountingCurrencyAmount);
        Assert.Equal(400m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
    }

    [Fact]
    public async Task Reversing_a_vendor_payment_reopens_the_invoice_it_had_settled()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorPayment(db, amount: 400m, withSettledInvoice: true, invoiceAmount: -1000m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendReversal(db).ReverseAsync("LJT-VPAY", Reversed, default);

        Assert.Equal(200, result.StatusCode);
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(0m, invoice.SettleAmountCur);
        var open = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 10);
        Assert.Equal(-1000m, open.AmountCur);
    }

    [Fact]
    public async Task The_vendor_payment_and_its_reversal_are_left_settled_against_each_other()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await NewVendReversal(db).ReverseAsync("LJT-VPAY", Reversed, default);

        var payment = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 20);
        var reversal = await db.Set<VendTrans>().AsNoTracking()
            .SingleAsync(x => x.RecId != 20 && x.TransType == LedgerTransType.Payment);
        Assert.Equal(-400m, reversal.AmountCur);
        Assert.Equal(0m, payment.AmountCur - payment.SettleAmountCur);
        Assert.Equal(0m, reversal.AmountCur - reversal.SettleAmountCur);
    }

    [Fact]
    public async Task A_vendor_payment_cannot_be_reversed_twice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        SeedVendorPayment(db, amount: 400m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = NewVendReversal(db);

        Assert.Equal(200, (await service.ReverseAsync("LJT-VPAY", Reversed, default)).StatusCode);
        var second = await service.ReverseAsync("LJT-VPAY", Reversed, default);

        Assert.Equal(422, second.StatusCode);
        Assert.Contains("already been reversed", second.Message);
    }

    // ---------- Fixture ----------

    /// <summary>
    /// Builds what CustPaymentService leaves behind: the posted journal and line, the payment
    /// CustTrans, its bank movement, and optionally the invoice it settled.
    /// </summary>
    private static void SeedCustomerPayment(DbContext db, decimal amount, bool withSettledInvoice = false,
        decimal invoiceAmount = 0m)
    {
        var settled = withSettledInvoice ? amount : 0m;
        if (withSettledInvoice)
        {
            db.Add(new CustTrans
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "C-001", Invoice = "INV-1", Voucher = "VOU-INV",
                TransDate = new DateTime(2026, 1, 5), DueDate = new DateTime(2026, 1, 31), CurrencyCode = "SAR",
                AmountCur = invoiceAmount, AmountMst = invoiceAmount, ReportingCurrencyAmount = invoiceAmount,
                SettleAmountCur = settled, SettleAmountMst = settled, TransType = LedgerTransType.Sales
            });
            db.Add(new CustTransOpen
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 10,
                TransDate = new DateTime(2026, 1, 5), DueDate = new DateTime(2026, 1, 31),
                AmountCur = invoiceAmount - settled, AmountMst = invoiceAmount - settled,
                ReportingCurrencyAmount = invoiceAmount - settled
            });
        }

        db.Add(new CustTrans
        {
            RecId = 20, DataAreaId = "dat", AccountNum = "C-001", OrderAccount = "C-001", Voucher = "VOU-PAY",
            DocumentNum = "LJT-PAY", TransDate = Posted, DueDate = Posted, CurrencyCode = "SAR",
            AmountCur = -amount, AmountMst = -amount, ReportingCurrencyAmount = -amount,
            SettleAmountCur = -settled, SettleAmountMst = -settled, TransType = LedgerTransType.Payment,
            Closed = withSettledInvoice ? Posted : default,
            Settlement = withSettledInvoice ? CustSettlementStatus.Settled : CustSettlementStatus.None
        });
        if (!withSettledInvoice)
            db.Add(new CustTransOpen
            {
                RecId = 20, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 20, TransDate = Posted,
                DueDate = Posted, AmountCur = -amount, AmountMst = -amount, ReportingCurrencyAmount = -amount
            });
        if (withSettledInvoice)
        {
            db.Add(new CustSettlement
            {
                RecId = 1, DataAreaId = "dat", AccountNum = "C-001", TransDate = Posted,
                SettlementVoucher = "CSET-000001", TransCompany = "dat", TransType = LedgerTransType.Sales,
                TransRecId = 10, OffsetRecId = 20, OffsetAccountNum = "C-001", OffsetCompany = "dat",
                SettleAmountCur = settled, SettleAmountMst = settled, SettleAmountReporting = settled,
                DueDate = Posted, ClosedDate = Posted, CanBeReversed = NoYes.Yes
            });
            db.Add(new CustSettlement
            {
                RecId = 2, DataAreaId = "dat", AccountNum = "C-001", TransDate = Posted,
                SettlementVoucher = "CSET-000001", TransCompany = "dat", TransType = LedgerTransType.Payment,
                TransRecId = 20, OffsetRecId = 10, OffsetAccountNum = "C-001", OffsetCompany = "dat",
                SettleAmountCur = -settled, SettleAmountMst = -settled, SettleAmountReporting = -settled,
                DueDate = Posted, ClosedDate = Posted, CanBeReversed = NoYes.Yes
            });
        }

        db.Add(new LedgerJournalTable
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "LJT-PAY", JournalName = "GEN",
            Name = "Customer payment", JournalType = LedgerJournalType.CustPayment, NumOfLines = 1,
            CurrencyCode = "SAR", JournalTotalCredit = amount, JournalTotalDebit = amount,
            OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = 1,
            Posted = NoYes.Yes, PostedDateTime = Posted
        });
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "LJT-PAY", LineNum = 1, Voucher = "VOU-PAY",
            TransDate = Posted, Company = "dat", AccountType = LedgerJournalACType.Cust,
            LedgerDimension = 2, LedgerDimensionName = "C-001", AmountCurCredit = amount, CurrencyCode = "SAR",
            OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = 1, OffsetCompany = "dat",
            Txt = "Payment from C-001", DocumentNum = "LJT-PAY", DocumentDate = Posted, CustTransId = 20
        });
        db.Add(new BankAccountTrans
        {
            RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", TransDate = Posted, Voucher = "VOU-PAY",
            Txt = "Customer payment", LedgerTransType = LedgerTransType.Payment, CurrencyCode = "SAR",
            BankTransCurrencyCode = "SAR", AmountCur = amount, AmountMst = amount,
            AmountReportingCurrency = amount, BankTransAmountCur = amount, LedgerDimension = 1,
            SourceTableId = FinanceTableNumbers.LedgerJournalTrans, SourceRecId = 1
        });
    }

    private static void SeedVendorPayment(DbContext db, decimal amount, bool withSettledInvoice = false,
        decimal invoiceAmount = 0m)
    {
        var settled = withSettledInvoice ? amount : 0m;
        if (withSettledInvoice)
        {
            db.Add(new VendTrans
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "V-001", Invoice = "VINV-1", Voucher = "VOU-VINV",
                TransDate = new DateTime(2026, 1, 5), DueDate = new DateTime(2026, 1, 31), CurrencyCode = "SAR",
                AmountCur = invoiceAmount, AmountMst = invoiceAmount,
                SettleAmountCur = -settled, SettleAmountMst = -settled, TransType = LedgerTransType.Purch
            });
            db.Add(new VendTransOpen
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 10,
                TransDate = new DateTime(2026, 1, 5), DueDate = new DateTime(2026, 1, 31),
                AmountCur = invoiceAmount + settled, AmountMst = invoiceAmount + settled
            });
        }

        db.Add(new VendTrans
        {
            RecId = 20, DataAreaId = "dat", AccountNum = "V-001", Voucher = "VOU-VPAY", TransDate = Posted,
            DueDate = Posted, CurrencyCode = "SAR", AmountCur = amount, AmountMst = amount,
            SettleAmountCur = settled, SettleAmountMst = settled, TransType = LedgerTransType.Payment,
            Closed = withSettledInvoice ? Posted : default
        });
        if (!withSettledInvoice)
            db.Add(new VendTransOpen
            {
                RecId = 20, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 20, TransDate = Posted,
                DueDate = Posted, AmountCur = amount, AmountMst = amount
            });
        if (withSettledInvoice)
            db.Add(new VendSettlement
            {
                RecId = 1, DataAreaId = "dat", AccountNum = "V-001", InvoiceTransRecId = 10,
                PaymentTransRecId = 20, TransDate = Posted, Voucher = "VOU-VPAY",
                AmountCur = settled, AmountMst = settled, CanBeReversed = NoYes.Yes
            });

        db.Add(new LedgerJournalTable
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "LJT-VPAY", JournalName = "GEN",
            Name = "Vendor payment", JournalType = LedgerJournalType.VendDisbursement, NumOfLines = 1,
            CurrencyCode = "SAR", JournalTotalCredit = amount, JournalTotalDebit = amount,
            OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = 1,
            Posted = NoYes.Yes, PostedDateTime = Posted
        });
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "LJT-VPAY", LineNum = 1, Voucher = "VOU-VPAY",
            TransDate = Posted, Company = "dat", AccountType = LedgerJournalACType.Vend,
            LedgerDimension = 3, LedgerDimensionName = "V-001", AmountCurDebit = amount, CurrencyCode = "SAR",
            OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = 1, OffsetCompany = "dat",
            Txt = "Payment to V-001", DocumentNum = "LJT-VPAY", DocumentDate = Posted, VendTransId = 20
        });
        db.Add(new BankAccountTrans
        {
            RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", TransDate = Posted, Voucher = "VOU-VPAY",
            Txt = "Vendor payment", LedgerTransType = LedgerTransType.Payment, CurrencyCode = "SAR",
            BankTransCurrencyCode = "SAR", AmountCur = -amount, AmountMst = -amount,
            AmountReportingCurrency = -amount, BankTransAmountCur = -amount, LedgerDimension = 1,
            SourceTableId = FinanceTableNumbers.LedgerJournalTrans, SourceRecId = 1
        });
    }

    private static CustPaymentReversalService NewCustReversal(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, PaymentReversalSequenceProxy>());

    private static VendPaymentReversalService NewVendReversal(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, PaymentReversalSequenceProxy>());

    private static async Task<PaymentReversalContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PaymentReversalContext(
            new DbContextOptionsBuilder<PaymentReversalContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new Ledger { RecId = 1, DataAreaId = "dat", ChartOfAccounts = 10, FiscalCalendar = 1, AccountingCurrency = "SAR" },
            // Only January is open, so a February reversal date has no period to post into.
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) },
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new MainAccount { RecId = 1, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "110100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "130100" },
            new MainAccount { RecId = 3, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "210100" },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "110100", DisplayValue = "110100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "130100", DisplayValue = "130100" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "210100", DisplayValue = "210100" },
            new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM" },
            new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 },
            new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001" },
            new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 3 });
        db.SaveChanges();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class PaymentReversalContext(DbContextOptions<PaymentReversalContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement), typeof(CustTable),
                typeof(CustParameters), typeof(CustLedgerAccounts),
                typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(VendInvoiceJour), typeof(VendTable),
                typeof(VendLedgerAccounts),
                typeof(VendParameters),
                typeof(LedgerJournalTable), typeof(LedgerJournalTrans), typeof(BankAccountTrans),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(MainAccountLegalEntity), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry),
                typeof(SubledgerVoucherGeneralJournalEntry)];

            // The customer and vendor tables reach the party and address graph, so everything not
            // named here is ignored outright rather than discovered one failure at a time.
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

    public class PaymentReversalSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name != nameof(ISysNumberSequenceService.NextAsync))
                throw new NotSupportedException(method?.Name);
            var entity = (string)args![0]!;
            _n++;
            var prefix = entity switch
            {
                "LedgerJournalTable" => "LJT-REV",
                "Voucher" => "VOU-REV",
                "CustSettlement" => "CSET",
                _ => entity
            };
            return Task.FromResult(new NextSequenceResultDto
                { EntityName = entity, Value = _n, Code = $"{prefix}-{_n:D4}" });
        }
    }

    public class PaymentReversalRepositoryProxy<T> : DispatchProxy where T : class
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
            var repository = DispatchProxy.Create<IGenericRepository<T>, PaymentReversalRepositoryProxy<T>>();
            ((PaymentReversalRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
