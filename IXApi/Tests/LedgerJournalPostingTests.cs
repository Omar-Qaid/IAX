using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Features;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public class LedgerJournalPostingTests
{
    // Case A of the core general ledger cycle: owner funding, two explicit lines under one voucher.
    [Fact]
    public async Task Posts_two_line_voucher_to_general_journal_entry_and_account_entries()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(
            Line(1, "V1", bank: true, debit: 100000m),
            Line(2, "V1", bank: false, credit: 100000m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var result = await new LedgerJournalPosting(db).PostAsync("dat", header, default);
        await db.SaveChangesAsync();

        Assert.Equal("J1", result.JournalNum);
        Assert.Equal(1, result.VouchersPosted);
        Assert.Equal(2, result.AccountEntriesPosted);
        Assert.Equal(100000m, result.TotalDebit);
        Assert.Equal(100000m, result.TotalCredit);

        var entry = await db.Set<GeneralJournalEntry>().AsNoTracking().SingleAsync();
        Assert.Equal("V1", entry.SubledgerVoucher);
        Assert.Equal("J1", entry.JournalNumber);
        Assert.Equal(new DateTime(2026, 1, 1), entry.AccountingDate);
        Assert.Equal(1, entry.FiscalCalendarPeriod);
        Assert.Equal(CurrentToOperationsPostingLayer.Current, entry.PostingLayer);

        var postings = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, postings.Count);
        Assert.All(postings, x => Assert.Equal(entry.RecId, x.GeneralJournalEntry));
        Assert.All(postings, x => Assert.Equal(LedgerPostingType.LedgerJournal, x.PostingType));
        Assert.Equal(0m, postings.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(100000m, postings.Single(x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        Assert.Equal(NoYes.No, postings.Single(x => x.LedgerDimension == 1).IsCredit);
        Assert.Equal(-100000m, postings.Single(x => x.LedgerDimension == 2).AccountingCurrencyAmount);
        Assert.Equal(NoYes.Yes, postings.Single(x => x.LedgerDimension == 2).IsCredit);

        var association = await db.Set<SubledgerVoucherGeneralJournalEntry>().AsNoTracking().SingleAsync();
        Assert.Equal(entry.RecId, association.GeneralJournalEntry);
        Assert.Equal("V1", association.Voucher);

        var posted = await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync();
        Assert.Equal(NoYes.Yes, posted.Posted);
        Assert.Equal(0m, posted.JournalBalance);
        Assert.All(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync(),
            x => Assert.Equal(NoYes.Yes, x.NoEdit));

        // A posted journal is never posted twice.
        var reloaded = await db.Set<LedgerJournalTable>().SingleAsync();
        var again = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", reloaded, default));
        Assert.Contains("already posted", again.Message);
    }

    // A line with an offset is one entered line and two posted account entries under one voucher.
    [Fact]
    public async Task Posts_offset_line_as_two_account_entries_under_one_voucher()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        var line = Line(1, "V2", bank: true, debit: 20000m);
        line.OffsetLedgerDimension = 2;
        line.OffsetAccountType = LedgerJournalACType.Ledger;
        line.OffsetTxt = "Service revenue";
        db.Add(line);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();

        Assert.Equal(1, result.VouchersPosted);
        Assert.Equal(2, result.AccountEntriesPosted);
        var postings = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, postings.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(-20000m, postings.Single(x => x.LedgerDimension == 2).AccountingCurrencyAmount);
        Assert.Equal("Service revenue", postings.Single(x => x.LedgerDimension == 2).Text);
    }

    [Fact]
    public async Task Separate_vouchers_and_dates_post_as_separate_entries()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        var second = new DateTime(2026, 1, 31);
        db.AddRange(
            Line(1, "V1", bank: true, debit: 100m),
            Line(2, "V1", bank: false, credit: 100m),
            Line(3, "V2", bank: true, debit: 50m, date: second),
            Line(4, "V2", bank: false, credit: 50m, date: second));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();

        Assert.Equal(2, result.VouchersPosted);
        Assert.Equal(4, result.AccountEntriesPosted);
        var entries = await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal([new DateTime(2026, 1, 1), second], entries.OrderBy(x => x.AccountingDate).Select(x => x.AccountingDate));
    }

    [Theory]
    [InlineData("accountType", "only the Ledger account type")]
    // A foreign transaction currency is stopped by the exchange-rate gate in the setup check,
    // before reaching the scope guard: either way the journal is refused, never approximated.
    [InlineData("currency", "no exchange rate type is configured")]
    [InlineData("tax", "journal tax calculation")]
    [InlineData("company", "intercompany")]
    [InlineData("offsetAccountType", "only the Ledger offset account type")]
    [InlineData("canceled", "canceled")]
    [InlineData("unbalanced", "unbalanced")]
    public async Task Rejects_what_this_journal_path_does_not_implement(string scenario, string expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        var debit = Line(1, "V1", bank: true, debit: 100m);
        LedgerJournalTrans? credit = Line(2, "V1", bank: false, credit: 100m);
        switch (scenario)
        {
            case "accountType": debit.AccountType = LedgerJournalACType.Cust; break;
            case "currency": debit.CurrencyCode = "USD"; credit.CurrencyCode = "USD"; break;
            case "tax": debit.TaxCode = "VAT15"; break;
            // Both lines carry the foreign company so the voucher still balances within its group
            // and the intercompany guard is what refuses it.
            case "company": debit.Company = "other"; credit.Company = "other"; break;
            // One line with an offset balances on its own, so the offset guard is what refuses it.
            case "offsetAccountType":
                debit.OffsetLedgerDimension = 2;
                debit.OffsetAccountType = LedgerJournalACType.Vend;
                credit = null;
                break;
            case "canceled": debit.Cancel = NoYes.Yes; break;
            case "unbalanced": credit.AmountCurCredit = 40m; break;
        }
        db.Add(debit);
        if (credit != null) db.Add(credit);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", header, default));
        Assert.Contains(expected, error.Message);
        // A refused journal leaves no posted accounting behind.
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
        Assert.Equal(NoYes.No, (await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync()).Posted);
    }


    // Section 9.4 of the cycle: the reversal is its own journal and its own voucher; the original
    // stays posted and traceable, and nothing posted is edited or removed.
    [Fact]
    public async Task Reversal_posts_a_mirrored_journal_linked_to_the_original()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(
            Line(1, "V1", bank: true, debit: 900m),
            Line(2, "V1", bank: false, credit: 900m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = NewReversalService(db);
        var (result, error) = await service.ReverseAsync(1, new DateTime(2026, 1, 31), default);

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(1, result!.VouchersPosted);
        Assert.Equal(2, result.AccountEntriesPosted);
        Assert.Equal(900m, result.TotalDebit);
        Assert.Equal(900m, result.TotalCredit);

        db.ChangeTracker.Clear();
        var journals = await db.Set<LedgerJournalTable>().AsNoTracking().OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(2, journals.Count);
        var reversal = journals.Single(x => x.OriginalJournalNum.Length > 0);
        Assert.Equal("J1", reversal.OriginalJournalNum);
        Assert.Equal("dat", reversal.OriginalCompany);
        Assert.Equal(NoYes.Yes, reversal.Posted);
        Assert.Equal("GEN", reversal.JournalName);
        Assert.Contains("Reversal of J1", reversal.Name);
        // The original remains posted and untouched.
        Assert.Equal(NoYes.Yes, journals.Single(x => x.JournalNum == "J1").Posted);

        var entries = await db.Set<GeneralJournalEntry>().AsNoTracking().OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(new DateTime(2026, 1, 31), entries[1].AccountingDate);
        Assert.NotEqual(entries[0].SubledgerVoucher, entries[1].SubledgerVoucher);

        var reversed = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.GeneralJournalEntry == entries[1].RecId).ToListAsync();
        Assert.Equal(0m, reversed.Sum(x => x.AccountingCurrencyAmount));
        // The bank debit of 900 comes back as a credit of 900.
        Assert.Equal(-900m, reversed.Single(x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        Assert.Equal(900m, reversed.Single(x => x.LedgerDimension == 2).AccountingCurrencyAmount);
        // Original and reversal together net to zero.
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync())
            .Sum(x => x.AccountingCurrencyAmount));

        // A journal is reversed once, and a reversal is not itself reversible.
        var (_, second) = await service.ReverseAsync(1, new DateTime(2026, 1, 31), default);
        Assert.Contains("already been reversed", second);
        var (_, third) = await service.ReverseAsync(reversal.RecId, new DateTime(2026, 1, 31), default);
        Assert.Contains("itself a reversal", third);
    }

    [Fact]
    public async Task Reversal_refuses_an_unposted_journal_and_an_unknown_one()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(
            Line(1, "V1", bank: true, debit: 100m),
            Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = NewReversalService(db);
        var (result, error) = await service.ReverseAsync(1, null, default);
        Assert.Null(result);
        Assert.Contains("Only a posted journal", error);

        var (missing, noError) = await service.ReverseAsync(404, null, default);
        Assert.Null(missing);
        Assert.Null(noError);
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
    }

    private static LedgerJournalReversalService NewReversalService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>());


    // A main account the company blocks for manual entry cannot be posted from a general journal.
    [Fact]
    public async Task Refuses_an_account_blocked_for_manual_entry_in_this_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(
            new CompanyInfo { RecId = 50, DataAreaId = "dat" },
            new CompanyInfo { RecId = 51, DataAreaId = "other" },
            // Share capital is blocked in this company and open in the other one.
            new MainAccountLegalEntity { RecId = 1, MainAccount = 2, LegalEntity = 50, DoNotAllowManualEntry = NoYes.Yes },
            new MainAccountLegalEntity { RecId = 2, MainAccount = 2, LegalEntity = 51, DoNotAllowManualEntry = NoYes.No },
            Line(1, "V1", bank: true, debit: 100m),
            Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", header, default));
        Assert.Contains("does not allow manual entry", error.Message);
        Assert.Contains("310100", error.Message);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());

        // Lifting the block lets the same journal post.
        var block = await db.Set<MainAccountLegalEntity>().SingleAsync(x => x.RecId == 1);
        block.DoNotAllowManualEntry = NoYes.No;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var result = await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        Assert.Equal(1, result.VouchersPosted);
    }

    // The account's own controls are judged on the posted signed amount, so the line and its offset
    // are each checked against the account they actually hit.
    [Fact]
    public async Task Refuses_a_line_on_the_side_its_main_account_does_not_accept()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        db.AddRange(
            Line(1, "V1", bank: true, credit: 100m),
            Line(2, "V1", bank: false, debit: 100m));
        // Share capital is restricted to credit amounts, and this journal debits it.
        var shareCapital = await db.Set<MainAccount>().SingleAsync(x => x.RecId == 2);
        shareCapital.DebitCreditCheck = DebitCreditCheck.Check;
        shareCapital.DebitCreditProposal = DebitCreditProposal.Credit;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", header, default));
        Assert.Contains("credit amounts only", error.Message);
        Assert.Contains("310100", error.Message);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
    }

    // The payment reference was dropped at posting even though both sides carry the field.
    [Fact]
    public async Task A_required_payment_reference_is_enforced_and_reaches_the_posted_entry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        db.AddRange(
            Line(1, "V1", bank: true, debit: 100m),
            Line(2, "V1", bank: false, credit: 100m));
        var bank = await db.Set<MainAccount>().SingleAsync(x => x.RecId == 1);
        bank.MandatoryPaymentReference = NoYes.Yes;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", header, default));
        Assert.Contains("requires a payment reference", error.Message);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());

        var line = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.RecId == 1);
        line.PaymReference = "PAY-1";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var result = await new LedgerJournalPosting(db).PostAsync(
            "dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        Assert.Equal(1, result.VouchersPosted);

        db.ChangeTracker.Clear();
        var posted = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal("PAY-1", posted.Single(x => x.LedgerAccount == "110100").PaymentReference);
        // The other line carries its own reference, which is empty, and its account requires none.
        Assert.Equal(string.Empty, posted.Single(x => x.LedgerAccount == "310100").PaymentReference);
    }

    // A posting control the company switched on is refused, not skipped: posting looser than the
    // configured control would be worse than refusing the journal.
    [Fact]
    public async Task Refuses_a_journal_when_a_posting_control_is_switched_on_but_not_applied()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(
            new IAX.IXApi.Modules.Finance.Entities.LedgerParameters
            {
                RecId = 1, DataAreaId = "dat", CheckVoucher = NoYes.Yes
            },
            Line(1, "V1", bank: true, debit: 100m),
            Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", header, default));
        Assert.Contains("Check voucher", error.Message);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());

        // Turning the control off lets the same journal post; a tolerance never blocks posting.
        var parameters = await db.Set<IAX.IXApi.Modules.Finance.Entities.LedgerParameters>().SingleAsync();
        parameters.CheckVoucher = NoYes.No;
        parameters.MaxRoundingDifferenceMst = 0.05m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var result = await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        Assert.Equal(1, result.VouchersPosted);
    }

    // The reversal carries IsCorrection only when the company's parameters ask for it.
    [Fact]
    public async Task Reversal_marks_correction_when_the_ledger_parameters_require_it()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(
            new IAX.IXApi.Modules.Finance.Entities.LedgerParameters
            {
                RecId = 1, DataAreaId = "dat", TransactionReversalCorrection = NoYes.Yes
            },
            Line(1, "V1", bank: true, debit: 400m),
            Line(2, "V1", bank: false, credit: 400m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);
        Assert.Null(error);
        Assert.NotNull(result);

        db.ChangeTracker.Clear();
        var entries = await db.Set<GeneralJournalEntry>().AsNoTracking().OrderBy(x => x.RecId).ToListAsync();
        var original = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.GeneralJournalEntry == entries[0].RecId).ToListAsync();
        var reversal = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.GeneralJournalEntry == entries[1].RecId).ToListAsync();
        Assert.All(reversal, x => Assert.Equal(NoYes.Yes, x.IsCorrection));
        // The original voucher is not rewritten.
        Assert.All(original, x => Assert.Equal(NoYes.No, x.IsCorrection));
    }

    private static LedgerJournalTrans Line(long recId, string voucher, bool bank,
        decimal debit = 0m, decimal credit = 0m, DateTime? date = null) => new()
        {
            RecId = recId, DataAreaId = "dat", Company = "dat", JournalNum = "J1", LineNum = recId,
            Voucher = voucher, TransDate = date ?? new DateTime(2026, 1, 1), CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Ledger, LedgerDimension = bank ? 1 : 2,
            AmountCurDebit = debit, AmountCurCredit = credit,
            Txt = bank ? "Bank ledger control" : "Share capital"
        };

    private static void Seed(PostingContext db)
    {
        db.AddRange(
            new LedgerJournalTable { RecId = 1, DataAreaId = "dat", JournalNum = "J1", JournalName = "GEN" },
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

    private static async Task<PostingContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PostingContext(new DbContextOptionsBuilder<PostingContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class PostingContext(DbContextOptions<PostingContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(LedgerJournalTable), typeof(LedgerJournalName), typeof(LedgerJournalTrans),
                typeof(CompanyInfo), typeof(MainAccountLegalEntity), typeof(IAX.IXApi.Modules.Finance.Entities.LedgerParameters),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry),
                typeof(SubledgerVoucherGeneralJournalEntry), typeof(ExchangeRateType),
                typeof(ExchangeRateCurrencyPair), typeof(ExchangeRate)];
            // CompanyInfo navigates to the party tables, which this harness does not map.
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

    public class TestSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            method?.Name == nameof(ISysNumberSequenceService.NextAsync)
                ? Task.FromResult(new NextSequenceResultDto { EntityName = "X", Value = ++_n, Code = $"X-{_n:D6}" })
                : throw new NotSupportedException(method?.Name);
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
