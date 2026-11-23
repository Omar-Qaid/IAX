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
    [Fact]
    public async Task Validation_groups_a_blank_line_with_the_single_entered_voucher()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "EXISTING", bank: true, debit: 100m), Line(2, "", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        db.ChangeTracker.Clear();
        var service = new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser());
        var validation = await service.ValidateAsync(1, default);
        Assert.NotNull(validation);
        Assert.True(validation!.Valid, string.Join("; ", validation.Errors));
        var (posted, error) = await service.PostAsync(1, default);
        Assert.Null(error);
        Assert.NotNull(posted);
        Assert.Equal(1, posted!.VouchersPosted);
    }

    [Theory]
    [InlineData(LedgerJournalVoucherChange.None, true)]
    [InlineData(LedgerJournalVoucherChange.Change, false)]
    public async Task Validation_predicts_blank_voucher_grouping_without_allocating_numbers(LedgerJournalVoucherChange change, bool valid)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.Set<LedgerJournalName>().Local.Single().NewVoucher = change;
        db.AddRange(Line(1, "", bank: true, debit: 100m), Line(2, "", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        db.ChangeTracker.Clear();
        var before = (await db.Set<SysNumberSequence>().AsNoTracking().SingleAsync()).NextRec;
        var result = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).ValidateAsync(1, default);
        Assert.NotNull(result);
        Assert.Equal(valid, result!.Valid);
        Assert.DoesNotContain(result.Errors, error => error.Contains("voucher is required"));
        Assert.Equal(before, (await db.Set<SysNumberSequence>().AsNoTracking().SingleAsync()).NextRec);
        Assert.All(await db.Set<LedgerJournalTrans>().AsNoTracking().ToListAsync(), line => Assert.Equal("", line.Voucher));
    }

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

    [Fact]
    public async Task Allocation_at_posting_replaces_each_temporary_voucher_group_from_configured_sequence()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        // Seed only stages its rows; without this the queries below read an empty database.
        await db.SaveChangesAsync();
        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        header.VoucherAllocatedAtPosting = NoYes.Yes;
        var journalName = await db.Set<LedgerJournalName>().SingleAsync();
        journalName.VoucherAllocatedAtPosting = NoYes.Yes;
        journalName.NumberSequenceTable = 90;
        db.Add(new SysNumberSequence
        {
            RecId = 90,
            DataAreaId = "dat",
            NumberSequence = "GEN-V",
            Txt = "General journal voucher",
            Format = "GV-######",
            AnnotatedFormat = "{PREFIX}-{SEQ}",
            Lowest = 100,
            NextRec = 100,
            Highest = 999,
            IsActive = true,
            InUse = 1,
            Manual = 0,
            NoIncrement = 0,
            Blocked = 0
        });
        db.AddRange(
            Line(1, "TMP-1", bank: true, debit: 100m),
            Line(2, "TMP-1", bank: false, credit: 100m),
            Line(3, "TMP-2", bank: true, debit: 200m),
            Line(4, "TMP-2", bank: false, credit: 200m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new LedgerJournalPosting(db).PostAsync(
            "dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();

        Assert.Equal(2, result.VouchersPosted);
        var lines = await db.Set<LedgerJournalTrans>().AsNoTracking().OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(lines[0].Voucher, lines[1].Voucher);
        Assert.Equal(lines[2].Voucher, lines[3].Voucher);
        Assert.NotEqual(lines[0].Voucher, lines[2].Voucher);
        Assert.DoesNotContain(lines, x => x.Voucher.StartsWith("TMP-", StringComparison.Ordinal));
        Assert.Equal(102, (await db.Set<SysNumberSequence>().AsNoTracking().SingleAsync()).NextRec);
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
        Assert.Equal(20000m, result.TotalDebit);
        Assert.Equal(20000m, result.TotalCredit);
        var postedHeader = await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync();
        Assert.Equal(0m, postedHeader.JournalBalance);
        Assert.Equal(20000m, postedHeader.JournalTotalDebitReportingCurrency);
        Assert.Equal(20000m, postedHeader.JournalTotalCreditReportingCurrency);
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

    [Fact]
    public async Task Voucher_allocation_refreshes_stale_counter_and_preserves_pending_increments()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        await db.Set<SysNumberSequence>().SingleAsync();
        await db.Set<SysNumberSequence>().ExecuteUpdateAsync(s => s.SetProperty(x => x.NextRec, 105));
        var first = await LedgerJournalVoucherNumberSequence.NextAsync(db, "dat", 90, default);
        var second = await LedgerJournalVoucherNumberSequence.NextAsync(db, "dat", 90, default);
        Assert.NotEqual(first, second);
        Assert.Equal(107, db.Set<SysNumberSequence>().Local.Single().NextRec);
        await db.SaveChangesAsync();
        Assert.Equal(107, (await db.Set<SysNumberSequence>().AsNoTracking().SingleAsync()).NextRec);
    }

    /// <summary>
    /// "Voucher change: none" is the journal name default, and it means a line entered without a
    /// voucher continues the one the journal already carries. Such a line used to be allocated a
    /// number of its own, which split a balanced two-line entry into two single-line vouchers and
    /// refused the posting as unbalanced. It does not make the journal one voucher only: the core
    /// cycle reference says outright not to assume a journal gets a single voucher, and
    /// <see cref="Allocation_at_posting_replaces_each_temporary_voucher_group_from_configured_sequence"/>
    /// still posts two.
    /// </summary>
    [Fact]
    public async Task A_line_without_a_voucher_continues_the_one_the_journal_already_carries()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        await ConfigureVoucherSequenceAsync(db);
        Assert.Equal(LedgerJournalVoucherChange.None,
            (await db.Set<LedgerJournalName>().AsNoTracking().SingleAsync()).NewVoucher);
        db.AddRange(
            Line(1, "GV-000001", bank: true, debit: 100m),
            Line(2, string.Empty, bank: false, credit: 100m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new LedgerJournalPosting(db).PostAsync(
            "dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();

        Assert.Equal(1, result.VouchersPosted);
        var lines = await db.Set<LedgerJournalTrans>().AsNoTracking().OrderBy(x => x.RecId).ToListAsync();
        Assert.All(lines, x => Assert.Equal("GV-000001", x.Voucher));
        // Nothing was drawn from the sequence, because no new voucher was needed.
        Assert.Equal(100, (await db.Set<SysNumberSequence>().AsNoTracking().SingleAsync()).NextRec);
    }
}
