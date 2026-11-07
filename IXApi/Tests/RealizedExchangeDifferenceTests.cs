using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
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
/// Realized exchange gain and loss when a customer payment settles a foreign-currency invoice at a
/// different rate from the one the invoice was posted at.
///
/// <para>
/// The posting was written on 2026-11-02 and went a day with no test at all, which made it the
/// largest unproven amount-computing path in the finance modules - and it writes to the ledger.
/// These tests pin the two directions and the account lookup.
/// </para>
///
/// <para>
/// The arithmetic, with SAR as the accounting currency and a 1,000 USD invoice booked at 3.75
/// (3,750 SAR):
/// </para>
/// <list type="bullet">
/// <item>
/// Paid when USD is worth 3.60: the bank receives 3,600 SAR but the receivable being cleared was
/// carried at 3,750, so 150 SAR is <b>lost</b>. The voucher is Dr Bank 3,600, Cr Customer 3,750,
/// Dr realized loss 150.
/// </item>
/// <item>
/// Paid when USD is worth 3.90: the bank receives 3,900 against the same 3,750 receivable, so 150
/// SAR is <b>gained</b>. Dr Bank 3,900, Cr Customer 3,750, Cr realized gain 150.
/// </item>
/// </list>
///
/// <para>
/// The ledger entry is derived as the plug that balances the posted lines rather than from the
/// per-invoice differences, so it is exact regardless of per-amount rounding. The transaction
/// currency amounts are unaffected either way: 1,000 USD clears 1,000 USD.
/// </para>
/// </summary>
public class RealizedExchangeDifferenceTests
{
    private const decimal InvoiceUsd = 1000m;
    private const decimal InvoiceRate = 3.75m;
    private static readonly DateTime InvoiceDate = new(2026, 1, 1);
    private static readonly DateTime PaymentDate = new(2026, 1, 20);

    [Fact]
    public async Task Final_payment_clears_the_exact_accounting_remainder_after_partial_rounding()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: InvoiceRate);
        var service = NewService(db);
        var amounts = new[] { 333.33m, 333.33m, 333.34m };
        for (var index = 0; index < amounts.Length; index++)
        {
            var result = await service.PostAsync(
                new("C-001", "BANK-1", amounts[index], PaymentDate, null, null, $"rounding-{index}"), default);
            Assert.True(result.StatusCode == 200, result.Message);
        }
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(3750m, invoice.SettleAmountMst);
        Assert.Equal(3750m, invoice.SettleAmountReporting);
        var settlements = await db.Set<CustSettlement>().AsNoTracking()
            .Where(x => x.TransRecId == 10).OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(new[] { 1249.99m, 1249.99m, 1250.02m }, settlements.Select(x => x.SettleAmountMst).ToArray());
        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        var customerPostings = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.PostingType == LedgerPostingType.CustBalance).ToListAsync();
        Assert.Equal(-3750m, customerPostings.Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task A_second_partial_payment_uses_the_reporting_currency_open_balance()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: InvoiceRate);
        (await db.Set<Ledger>().SingleAsync()).ReportingCurrency = "USD";
        (await db.Set<CustTrans>().SingleAsync(x => x.RecId == 10)).ReportingCurrencyAmount = InvoiceUsd;
        (await db.Set<CustTransOpen>().SingleAsync(x => x.RefRecId == 10)).ReportingCurrencyAmount = InvoiceUsd;
        await db.SaveChangesAsync();
        var numbers = DispatchProxy.Create<ISysNumberSequenceService, ExchangeSequenceProxy>();
        var company = new TestCompany();
        var service = new CustPaymentService(new TestUnitOfWork(db), company, numbers);
        var half = InvoiceUsd / 2;

        var first = await service.PostAsync(new("C-001", "BANK-1", half, PaymentDate, null, null, "reporting-1"), default);
        Assert.True(first.StatusCode == 200, first.Message);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 10);
        Assert.Equal(half, open.ReportingCurrencyAmount);
        Assert.Equal(1875m, open.AmountMst);

        var second = await service.PostAsync(new("C-001", "BANK-1", half, PaymentDate, null, null, "reporting-2"), default);
        Assert.True(second.StatusCode == 200, second.Message);
        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(InvoiceUsd, invoice.SettleAmountReporting);
        Assert.Equal(3750m, invoice.SettleAmountMst);

        var secondSettlement = await db.Set<CustSettlement>().AsNoTracking()
            .Where(x => x.TransRecId == 10).OrderByDescending(x => x.RecId).FirstAsync();
        var reversal = await new CustSettlementReversalService(new TestUnitOfWork(db), company, numbers)
            .ReverseAsync(secondSettlement.SettlementVoucher, PaymentDate.AddDays(1), default);
        Assert.True(reversal.StatusCode == 200, reversal.Message);
        open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == 10);
        Assert.Equal(half, open.ReportingCurrencyAmount);
        Assert.Equal(1875m, open.AmountMst);
    }

    /// <summary>Cash worth less than the receivable it cleared: a realized loss, posted as a debit.</summary>
    [Fact]
    public async Task A_payment_at_a_weaker_rate_posts_a_realized_loss_and_the_voucher_balances()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.60m);

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", InvoiceUsd, PaymentDate, null, null, "req-1"), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        // The voucher balances in the accounting currency, which is the property the plug exists for.
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(3600m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
        // The receivable clears at the value the invoice carried, not at the payment's rate.
        Assert.Equal(-3750m, entries.Single(x => x.PostingType == LedgerPostingType.CustBalance).AccountingCurrencyAmount);
        var loss = entries.Single(x => x.PostingType == LedgerPostingType.ExchRateLoss);
        Assert.Equal(150m, loss.AccountingCurrencyAmount); // a debit: the loss is an expense
        Assert.Equal("610400", loss.LedgerAccount);
        // No gain entry was posted, and the difference is recorded on the payment transaction.
        Assert.DoesNotContain(entries, x => x.PostingType == LedgerPostingType.ExchRateGain);
        var payment = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.TransType == LedgerTransType.Payment);
        Assert.Equal(150m, payment.CustExchAdjustmentRealized);
        // The invoice is fully settled in its own currency: 1,000 USD cleared 1,000 USD.
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(InvoiceUsd, invoice.SettleAmountCur);
        Assert.Equal(CustSettlementStatus.Settled, invoice.Settlement);
        Assert.Empty(await db.Set<CustTransOpen>().AsNoTracking().ToListAsync());
        // The realized difference is recorded on the settlement row too.
        var invoiceSettlement = await db.Set<CustSettlement>().AsNoTracking().SingleAsync(x => x.TransRecId == 10);
        var paymentSettlement = await db.Set<CustSettlement>().AsNoTracking().SingleAsync(x => x.TransRecId == payment.RecId);
        Assert.Equal(150m, invoiceSettlement.ExchAdjustment);
        Assert.Equal(1000m, invoiceSettlement.SettleAmountCur);
        Assert.Equal(3750m, invoiceSettlement.SettleAmountMst);
        Assert.Equal(-1000m, paymentSettlement.SettleAmountCur);
        Assert.Equal(-3600m, paymentSettlement.SettleAmountMst);
        var statement = await new CustTransService(new TestUnitOfWork(db), new TestUser())
            .GetStatementAsync("C-001", PaymentDate, PaymentDate, default);
        Assert.Equal(3750m, statement.OpeningBalanceMst);
        Assert.Equal(0m, statement.ClosingBalanceMst);
        var receipt = Assert.Single(statement.Lines);
        Assert.Equal(-1000m, receipt.AmountCur);
        Assert.Equal(-3750m, receipt.AmountMst);
    }

    /// <summary>Cash worth more than the receivable it cleared: a realized gain, posted as a credit.</summary>
    [Fact]
    public async Task A_payment_at_a_stronger_rate_posts_a_realized_gain_and_the_voucher_balances()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.90m);

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", InvoiceUsd, PaymentDate, null, null, "req-1"), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(3900m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
        Assert.Equal(-3750m, entries.Single(x => x.PostingType == LedgerPostingType.CustBalance).AccountingCurrencyAmount);
        var gain = entries.Single(x => x.PostingType == LedgerPostingType.ExchRateGain);
        Assert.Equal(-150m, gain.AccountingCurrencyAmount); // a credit: the gain is income
        Assert.Equal("410400", gain.LedgerAccount);
        Assert.DoesNotContain(entries, x => x.PostingType == LedgerPostingType.ExchRateLoss);
        var payment = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.TransType == LedgerTransType.Payment);
        Assert.Equal(-150m, payment.CustExchAdjustmentRealized);
    }

    /// <summary>
    /// Paying at the rate the invoice was booked at realizes nothing, so no gain or loss entry is
    /// posted at all and the voucher is the plain Dr Bank / Cr Customer pair.
    /// </summary>
    [Fact]
    public async Task A_payment_at_the_invoice_rate_realizes_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: InvoiceRate);

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", InvoiceUsd, PaymentDate, null, null, "req-1"), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.DoesNotContain(entries, x => x.PostingType == LedgerPostingType.ExchRateGain
            || x.PostingType == LedgerPostingType.ExchRateLoss);
        var payment = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.TransType == LedgerTransType.Payment);
        Assert.Equal(0m, payment.CustExchAdjustmentRealized);
    }

    /// <summary>
    /// A currency-specific account overrides the ledger-wide one, which is D365's
    /// currency-override-then-ledger lookup.
    /// </summary>
    [Fact]
    public async Task A_currency_specific_loss_account_overrides_the_ledger_wide_one()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.60m);
        // 610500 is configured for USD specifically, alongside the ledger-wide 610400.
        db.Add(new MainAccount { RecId = 7, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "610500" });
        db.Add(new DimensionAttributeValueCombination
        { RecId = 7, DataAreaId = "dat", MainAccount = 7, MainAccountValue = "610500", DisplayValue = "610500" });
        db.Add(new CurrencyLedgerGainLossAccount
        {
            RecId = 1, DataAreaId = "dat", Ledger = 1, CurrencyCode = "USD",
            AccountType = CurrencyGainLossAccountType.RealizedLoss, LedgerDimension = 7
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", InvoiceUsd, PaymentDate, null, null, "req-1"), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var loss = (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync())
            .Single(x => x.PostingType == LedgerPostingType.ExchRateLoss);
        Assert.Equal("610500", loss.LedgerAccount);
        Assert.Equal(150m, loss.AccountingCurrencyAmount);
    }

    /// <summary>
    /// With no gain or loss account configured anywhere, the payment is refused by name rather than
    /// posting an unbalanced voucher or silently dropping the difference.
    /// </summary>
    [Fact]
    public async Task A_missing_loss_account_refuses_the_payment_by_name()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.60m, configureGainLoss: false);

        var result = await NewService(db).PostAsync(
            new CustPaymentPostDto("C-001", "BANK-1", InvoiceUsd, PaymentDate, null, null, "req-1"), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("No realized exchange loss account is configured", result.Message);
        // Nothing was posted, and the invoice is still open.
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
        Assert.Equal(InvoiceUsd, (await db.Set<CustTransOpen>().AsNoTracking().SingleAsync()).AmountCur);
    }

    // ---------- Accounts payable ----------

    /// <summary>
    /// The payable mirror, and the gap this closes: the vendor payment used to refuse any currency
    /// other than the accounting currency outright. A 1,000 USD payable booked at 3.75 (3,750 SAR)
    /// discharged when USD costs 3.60 means 3,600 SAR left the bank to clear a 3,750 liability, so
    /// 150 SAR is <b>gained</b>. Dr Vendor 3,750, Cr Bank 3,600, Cr realized gain 150.
    /// </summary>
    [Fact]
    public async Task A_vendor_payment_at_a_weaker_rate_posts_a_realized_gain_and_the_voucher_balances()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.60m);

        var result = await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", InvoiceUsd, "req-1", PaymentDate), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        // The payable discharges at the value the invoice carried, not at the payment's rate.
        Assert.Equal(3750m, entries.Single(x => x.PostingType == LedgerPostingType.VendBalance).AccountingCurrencyAmount);
        Assert.Equal(-3600m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
        var gain = entries.Single(x => x.PostingType == LedgerPostingType.ExchRateGain);
        Assert.Equal(-150m, gain.AccountingCurrencyAmount); // a credit: the gain is income
        Assert.Equal("410400", gain.LedgerAccount);
        Assert.DoesNotContain(entries, x => x.PostingType == LedgerPostingType.ExchRateLoss);
        // The invoice is fully settled in its own currency and closed.
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 20);
        Assert.Equal(-InvoiceUsd, invoice.SettleAmountCur);
        Assert.Equal(-3750m, invoice.SettleAmountMst);
        Assert.Equal(PaymentDate, invoice.Closed);
        Assert.Empty(await db.Set<VendTransOpen>().AsNoTracking().ToListAsync());
        // The bank transaction carries the translated accounting amount, not the document amount.
        Assert.Equal(-3600m, (await db.Set<BankAccountTrans>().AsNoTracking().SingleAsync()).AmountMst);
    }

    /// <summary>Paying when the currency has strengthened costs more than the payable carried: a loss.</summary>
    [Fact]
    public async Task A_vendor_payment_at_a_stronger_rate_posts_a_realized_loss()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.90m);

        var result = await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", InvoiceUsd, "req-1", PaymentDate), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(3750m, entries.Single(x => x.PostingType == LedgerPostingType.VendBalance).AccountingCurrencyAmount);
        Assert.Equal(-3900m, entries.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);
        var loss = entries.Single(x => x.PostingType == LedgerPostingType.ExchRateLoss);
        Assert.Equal(150m, loss.AccountingCurrencyAmount); // a debit: the loss is an expense
        Assert.Equal("610400", loss.LedgerAccount);
    }

    /// <summary>
    /// Reversing a settlement that realized an exchange difference is refused, and the refusal is
    /// atomic. <see cref="VendSettlementRestore"/> rebuilds SettleAmountCur/Mst and the open row but
    /// restores neither <c>VendTrans.VendExchAdjustmentRealized</c> nor the ledger: the gain or loss
    /// was posted with the payment voucher, so reopening the invoice here would leave it in GL with
    /// no settlement behind it and the vendor control account would stop tying to the subledger.
    /// Reversing the payment instead reverses the whole voucher, which is the supported path.
    /// </summary>
    [Fact]
    public async Task Reversing_a_settlement_that_realized_an_exchange_difference_is_refused_atomically()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.60m);
        Assert.Equal(200, (await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", InvoiceUsd, "req-1", PaymentDate), default)).StatusCode);
        db.ChangeTracker.Clear();
        var settlement = await db.Set<VendSettlement>().AsNoTracking().SingleAsync();
        Assert.NotEqual(0m, settlement.ExchAdjustment);

        var reversal = await new VendSettlementReversalService(new TestUnitOfWork(db), new TestCompany())
            .ReverseAsync(settlement.Voucher, PaymentDate.AddDays(1), default);

        Assert.Equal(422, reversal.StatusCode);
        Assert.Equal($"Settlement '{settlement.Voucher}' carries a realized exchange adjustment; "
            + "this reversal path does not reverse its ledger voucher.", reversal.Message);
        // Nothing moved: the invoice stays settled and keeps its realized adjustment.
        db.ChangeTracker.Clear();
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 20);
        Assert.Equal(-InvoiceUsd, invoice.SettleAmountCur);
        Assert.Equal(-3750m, invoice.SettleAmountMst);
        Assert.False(await db.Set<VendTransOpen>().AsNoTracking().AnyAsync(x => x.RefRecId == 20));
    }

    /// <summary>
    /// Translating the bank side as well would be bank revaluation, a separate feature, so a bank
    /// account in another currency is still refused by name on the vendor side too.
    /// </summary>
    [Fact]
    public async Task A_vendor_payment_from_a_bank_in_another_currency_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection, paymentRate: 3.60m);
        db.Add(new BankAccountTable { RecId = 2, DataAreaId = "dat", AccountId = "BANK-SAR", LedgerDimension = 1,
            CurrencyCode = "SAR", BankAccountStatus = BankAccountStatus.Active });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-SAR", InvoiceUsd, "req-1", PaymentDate), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("bank account currency must match", result.Message);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
    }

    // ---------- Fixture ----------

    private static VendPaymentService NewVendService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, ExchangeSequenceProxy>());


    private static CustPaymentService NewService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestCompany(),
            DispatchProxy.Create<ISysNumberSequenceService, ExchangeSequenceProxy>());

    /// <summary>
    /// A 1,000 USD invoice booked at <see cref="InvoiceRate"/> on the invoice date, and a second
    /// rate in force from the payment date so the payment translates differently.
    /// </summary>
    private static async Task<ExchangeContext> NewContextAsync(SqliteConnection connection,
        decimal paymentRate, bool configureGainLoss = true)
    {
        var db = new ExchangeContext(
            new DbContextOptionsBuilder<ExchangeContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var invoiceMst = InvoiceUsd * InvoiceRate;
        db.AddRange(
            new Ledger { RecId = 1, DataAreaId = "dat", ChartOfAccounts = 10, FiscalCalendar = 1,
                AccountingCurrency = "SAR", DefaultExchangeRateType = 1 },
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) },
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new MainAccount { RecId = 1, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "110100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "130100" },
            new MainAccount { RecId = 3, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "610400" },
            new MainAccount { RecId = 4, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "410400" },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "110100", DisplayValue = "110100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "130100", DisplayValue = "130100" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "610400", DisplayValue = "610400" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, MainAccountValue = "410400", DisplayValue = "410400" },
            // The customer and the bank both hold USD, so the bank side needs no revaluation.
            new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM", CurrencyCode = "USD" },
            new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 },
            new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 1,
                CurrencyCode = "USD", BankAccountStatus = BankAccountStatus.Active },
            new CustTrans
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "C-001", Invoice = "INV-1", Voucher = "VOU-INV",
                TransDate = InvoiceDate, DueDate = new DateTime(2026, 1, 31), CurrencyCode = "USD",
                AmountCur = InvoiceUsd, AmountMst = invoiceMst, ReportingCurrencyAmount = invoiceMst,
                TransType = LedgerTransType.Sales
            },
            new CustTransOpen
            {
                RecId = 10, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 10,
                TransDate = InvoiceDate, DueDate = new DateTime(2026, 1, 31),
                AmountCur = InvoiceUsd, AmountMst = invoiceMst, ReportingCurrencyAmount = invoiceMst
            },
            // The payable mirror: a 1,000 USD vendor invoice booked at the same rate. Payables are
            // credits, so its amounts are negative.
            new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", Currency = "USD" },
            new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 },
            new VendTrans
            {
                RecId = 20, DataAreaId = "dat", AccountNum = "V-001", Invoice = "VINV-1", Voucher = "VOU-VINV",
                TransDate = InvoiceDate, DueDate = new DateTime(2026, 1, 31), CurrencyCode = "USD",
                AmountCur = -InvoiceUsd, AmountMst = -invoiceMst, TransType = LedgerTransType.Purch
            },
            new VendTransOpen
            {
                RecId = 20, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 20,
                TransDate = InvoiceDate, DueDate = new DateTime(2026, 1, 31),
                AmountCur = -InvoiceUsd, AmountMst = -invoiceMst
            },
            new ExchangeRateType { RecId = 1, DataAreaId = "dat", Name = "Default", IsActive = true },
            new ExchangeRateCurrencyPair
            {
                RecId = 1, DataAreaId = "dat", ExchangeRateType = 1, FromCurrencyCode = "USD", ToCurrencyCode = "SAR",
                ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One, IsActive = true
            },
            // The invoice's rate, then the payment date's rate.
            new ExchangeRate { RecId = 1, DataAreaId = "dat", ExchangeRateCurrencyPair = 1,
                ExchangeRateValue = InvoiceRate, ValidFrom = InvoiceDate, IsActive = true },
            new ExchangeRate { RecId = 2, DataAreaId = "dat", ExchangeRateCurrencyPair = 1,
                ExchangeRateValue = paymentRate, ValidFrom = PaymentDate, IsActive = true });
        if (configureGainLoss)
            db.AddRange(
                new LedgerGainLossAccount { RecId = 1, DataAreaId = "dat", Ledger = 1,
                    AccountType = CurrencyGainLossAccountType.RealizedLoss, LedgerDimension = 3 },
                new LedgerGainLossAccount { RecId = 2, DataAreaId = "dat", Ledger = 1,
                    AccountType = CurrencyGainLossAccountType.RealizedGain, LedgerDimension = 4 });
        TestAccountStructures.AssignPermissive(db, "dat", 1);
        db.SaveChanges();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class ExchangeContext(DbContextOptions<ExchangeContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement), typeof(CustTable),
                typeof(CustParameters), typeof(CustLedgerAccounts), typeof(CashDisc),
                typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(VendTable),
                typeof(VendLedgerAccounts), typeof(VendParameters), typeof(VendPaymModeTable),
                typeof(LedgerJournalTable), typeof(LedgerJournalTrans),
                typeof(BankAccountTable), typeof(BankAccountTrans),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(MainAccountLegalEntity), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry),
                typeof(SubledgerVoucherGeneralJournalEntry),
                typeof(Currency), typeof(ExchangeRate), typeof(ExchangeRateType), typeof(ExchangeRateCurrencyPair),
                typeof(LedgerGainLossAccount), typeof(CurrencyLedgerGainLossAccount),
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

    public class ExchangeSequenceProxy : DispatchProxy
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

    public class ExchangeRepositoryProxy<T> : DispatchProxy where T : class
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
            var repository = DispatchProxy.Create<IGenericRepository<T>, ExchangeRepositoryProxy<T>>();
            ((ExchangeRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
