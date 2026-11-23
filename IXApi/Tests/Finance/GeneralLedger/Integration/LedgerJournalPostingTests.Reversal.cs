using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Features;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed partial class LedgerJournalPostingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Reversal_does_not_commit_unrelated_pending_changes(bool added)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        if (added)
        {
            var other = Line(0, "V2", bank: true, debit: 50m);
            other.JournalNum = "J2";
            db.Add(other);
        }
        else (await db.Set<MainAccount>().SingleAsync(x => x.RecId == 1)).Name = "Unsaved account change";

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);

        Assert.Null(result);
        Assert.Equal("Save or discard pending changes before reversing a journal.", error);
        Assert.Single(await db.Set<LedgerJournalTable>().AsNoTracking().ToListAsync());
        Assert.Single(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
        Assert.Equal(2, await db.Set<LedgerJournalTrans>().AsNoTracking().CountAsync());
        Assert.NotEqual("Unsaved account change", (await db.Set<MainAccount>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generic_journal_creation_cannot_forge_a_reversal_link(bool bulk)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        var service = new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser());
        var forged = new LedgerJournalTable
        {
            DataAreaId = "dat", JournalNum = "FAKE", JournalName = "GEN", OriginalJournalNum = "J1"
        };
        var error = bulk
            ? await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddRangeAsync([forged]))
            : await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(forged));
        Assert.Contains("only by the journal reversal process", error.Message);
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Reversal_refuses_a_subledger_transaction_settled_after_journal_posting(bool vendor)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
            TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
            AccountType = vendor ? LedgerJournalACType.Vend : LedgerJournalACType.Cust,
            LedgerDimensionName = vendor ? "V-001" : "C-001",
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
            AmountCurDebit = 100m
        });
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        // Settlement can occur later, even though the journal line never selected an invoice.
        if (vendor) (await db.Set<VendTrans>().SingleAsync()).SettleAmountCur = 50m;
        else (await db.Set<CustTrans>().SingleAsync()).SettleAmountCur = 50m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);
        Assert.Null(result);
        Assert.Contains(vendor ? "Reverse the vendor settlement" : "Reverse the customer settlement", error);
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync());
        Assert.Single(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Equal(100, (await db.Set<SysNumberSequence>().SingleAsync()).NextRec);
    }

    [Fact]
    public async Task Vendor_prepayment_reversal_nets_the_original_advance_account_to_zero()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.Set<VendLedgerAccounts>().Local.Single().ClearingLedgerDimension = 1;
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
            TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Vend, LedgerDimensionName = "V-001", Prepayment = NoYes.Yes,
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2, AmountCurDebit = 100m
        });
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);
        Assert.Null(error);
        Assert.NotNull(result);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.All(entries.GroupBy(x => x.MainAccount), account => Assert.Equal(0m, account.Sum(x => x.AccountingCurrencyAmount)));
        Assert.DoesNotContain(entries, x => x.LedgerDimension == 4);
        Assert.All(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync(), x => Assert.Equal(NoYes.Yes, x.Prepayment));
    }

    [Fact]
    public async Task Reversal_rolls_back_when_changed_posting_setup_would_leave_original_account_uncorrected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
            TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Vend, LedgerDimensionName = "V-001",
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2, AmountCurDebit = 100m
        });
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        (await db.Set<VendLedgerAccounts>().SingleAsync()).SummaryLedgerDimension = 1;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);
        Assert.Null(result);
        Assert.Contains("does not cancel the original", error);
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Single(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync());
        Assert.Single(await db.Set<VendTrans>().ToListAsync());
        Assert.Equal(100, (await db.Set<SysNumberSequence>().SingleAsync()).NextRec);
    }

    // Section 9.4 of the cycle: the reversal is its own journal and its own voucher; the original
    // stays posted and traceable, and nothing posted is edited or removed.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Reversal_posts_a_mirrored_journal_linked_to_the_original(bool nextPeriod)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(
            Line(1, "V1", bank: true, debit: 900m),
            Line(2, "V1", bank: false, credit: 900m));
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var reversalDate = nextPeriod ? new DateTime(2026, 2, 1) : new DateTime(2026, 1, 31);
        if (nextPeriod)
        {
            (await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync()).Status = FiscalPeriodStatus.OnHold;
            db.Add(new FiscalCalendarPeriod { RecId = 2, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = reversalDate, EndDate = new DateTime(2026, 2, 28) });
            db.Add(new LedgerFiscalCalendarPeriod { RecId = 2, Ledger = 1, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.Open });
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }
        var service = NewReversalService(db);
        var (result, error) = await service.ReverseAsync(1, reversalDate, default);

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
        Assert.Equal(reversalDate, entries[1].AccountingDate);
        Assert.Equal(new DateTime(2026, 1, 1), entries[0].AccountingDate);
        Assert.Equal(nextPeriod ? 2L : 1L, entries[1].FiscalCalendarPeriod);
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
        var (_, second) = await service.ReverseAsync(1, reversalDate, default);
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
        await ConfigureVoucherSequenceAsync(db);
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

    // The one-line customer payment: the customer is credited and the bank offset is debited, and
    // reversing the journal cancels both subledger movements as well as the ledger entries.
    [Fact]
    public async Task A_customer_payment_with_a_bank_offset_posts_and_reverses_on_both_subledgers()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        SeedOffsetAccounts(db);
        var line = Line(1, "V1", bank: false, credit: 100m);
        line.AccountType = LedgerJournalACType.Cust;
        line.LedgerDimension = 0;
        line.LedgerDimensionName = "C-001";
        line.OffsetAccountType = LedgerJournalACType.Bank;
        line.OffsetLedgerDimension = 12;
        db.Add(line);
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(-100m, (await db.Set<CustTrans>().AsNoTracking().SingleAsync()).AmountCur);
        Assert.Equal(100m, (await db.Set<BankAccountTrans>().AsNoTracking().SingleAsync()).AmountCur);

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(0m, (await db.Set<CustTrans>().AsNoTracking().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(0m, (await db.Set<BankAccountTrans>().AsNoTracking().ToListAsync()).Sum(x => x.AmountCur));
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.All(entries.GroupBy(x => x.LedgerDimension), account => Assert.Equal(0m, account.Sum(x => x.AccountingCurrencyAmount)));
    }

    // The settlement guard on reversal covers a vendor on the offset side, which the line does not
    // record by transaction id.
    [Fact]
    public async Task Reversal_is_refused_while_an_offset_vendor_transaction_is_settled()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        SeedOffsetAccounts(db);
        var line = Line(1, "V1", bank: false, debit: 100m);
        line.OffsetAccountType = LedgerJournalACType.Vend;
        line.OffsetLedgerDimension = 11;
        db.Add(line);
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        (await db.Set<VendTrans>().SingleAsync()).SettleAmountCur = -50m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);

        Assert.Null(result);
        Assert.Contains("Reverse the vendor settlement", error);
        Assert.Single(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
    }

    // GL-02: an account that requires a payment reference posts, and must also reverse. The mirrored
    // line dropped the reference, so the account's own control refused the reversal of an entry it
    // had just accepted.
    [Fact]
    public async Task A_journal_on_an_account_requiring_a_payment_reference_can_be_reversed()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        var debit = Line(1, "V1", bank: true, debit: 300m);
        debit.PaymReference = "PAY-001";
        var credit = Line(2, "V1", bank: false, credit: 300m);
        db.AddRange(debit, credit);
        db.Set<MainAccount>().Local.Single(x => x.RecId == 1).MandatoryPaymentReference = NoYes.Yes;
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (result, error) = await NewReversalService(db).ReverseAsync(1, new DateTime(2026, 1, 31), default);
        Assert.Null(error);
        Assert.NotNull(result);
        db.ChangeTracker.Clear();

        var reversal = await db.Set<LedgerJournalTable>().AsNoTracking()
            .SingleAsync(x => x.OriginalJournalNum == "J1");
        var mirrored = await db.Set<LedgerJournalTrans>().AsNoTracking()
            .Where(x => x.JournalNum == reversal.JournalNum).ToListAsync();
        // The mirrored line for the controlled account carries the reference forward; the other
        // line never had one and its account requires none.
        Assert.Equal("PAY-001", mirrored.Single(x => x.LineNum == 1).PaymReference);
        Assert.Equal(string.Empty, mirrored.Single(x => x.LineNum == 2).PaymReference);
        // The reversal nets the original to zero on the account that carries the control.
        var postedOnTheControlledAccount = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.MainAccount == 1).ToListAsync();
        Assert.Equal(2, postedOnTheControlledAccount.Count);
        Assert.Equal(0m, postedOnTheControlledAccount.Sum(x => x.AccountingCurrencyAmount));
    }
}
