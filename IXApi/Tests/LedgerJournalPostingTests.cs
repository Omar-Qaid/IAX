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

public class LedgerJournalPostingTests
{
    [Fact]
    public async Task A_journal_credit_closes_the_selected_invoice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var date = new DateTime(2026, 1, 15);
        db.Add(new CustTrans { RecId = 90, DataAreaId = "dat", AccountNum = "C-001", Invoice = "INV-90",
            Voucher = "IV-90", CurrencyCode = "SAR", TransDate = date, AmountCur = 100m, AmountMst = 100m,
            ReportingCurrencyAmount = 100m });
        db.Add(new CustTransOpen { DataAreaId = "dat", AccountNum = "C-001", RefRecId = 90,
            AmountCur = 100m, AmountMst = 100m, ReportingCurrencyAmount = 100m });
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
            TransDate = date, CurrencyCode = "SAR", AccountType = LedgerJournalACType.Cust,
            LedgerDimensionName = "C-001", OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
            AmountCurCredit = 100m, MarkedInvoiceRecId = 90, SettleVoucher = SettleVoucher.Selection
        });
        await db.SaveChangesAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var journalName = await db.Set<LedgerJournalName>().SingleAsync();
        journalName.WorkflowApproval = NoYes.Yes;
        await db.SaveChangesAsync();

        var posting = new LedgerJournalPosting(db);
        // Only persisted approval can authorize posting.
        header.WorkflowApprovalStatus = WorkflowApprovalStatus.Approved;
        var setup = await new LedgerJournalSetupCheck(db).CheckAsync("dat", header.RecId, default);
        Assert.Contains("Approve the journal through workflow before posting.", setup!.Errors);
        var errorBeforeApproval = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => posting.PostAsync("dat", header, default));
        Assert.Contains("Approve the journal", errorBeforeApproval.Message);
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Equal(100m, (await db.Set<CustTransOpen>().SingleAsync()).AmountCur);

        await db.SaveChangesAsync();
        await posting.PostAsync("dat", header, default);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        var settlements = await db.Set<CustSettlement>().ToListAsync();
        Assert.Equal(2, settlements.Count);
        Assert.Contains(settlements, x => x.TransRecId == 90 && x.SettleAmountCur == 100m);
        Assert.Contains(settlements, x => x.OffsetRecId == 90 && x.SettleAmountCur == -100m);
        var (reversal, error) = await NewReversalService(db).ReverseAsync(1, date, default);
        Assert.Null(reversal);
        Assert.Contains("Reverse the customer settlement", error);
    }

    [Fact]
    public async Task A_stale_marked_balance_rolls_back_journal_posting()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var date = new DateTime(2026, 1, 15);
        db.Add(new CustTrans { RecId = 90, DataAreaId = "dat", AccountNum = "C-001", Invoice = "INV-90",
            Voucher = "IV-90", CurrencyCode = "SAR", TransDate = date, AmountCur = 100m, AmountMst = 100m,
            ReportingCurrencyAmount = 100m });
        db.Add(new CustTransOpen { DataAreaId = "dat", AccountNum = "C-001", RefRecId = 90,
            AmountCur = 75m, AmountMst = 75m, ReportingCurrencyAmount = 75m });
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
            TransDate = date, CurrencyCode = "SAR", AccountType = LedgerJournalACType.Cust,
            LedgerDimensionName = "C-001", OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
            AmountCurCredit = 100m, MarkedInvoiceRecId = 90, SettleVoucher = SettleVoucher.Selection
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser(),
            DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>());
        var (result, error) = await service.PostAsync(1, default);

        Assert.Null(result);
        Assert.Contains("open balance matching", error);
        Assert.Equal(NoYes.No, (await db.Set<LedgerJournalTable>().SingleAsync()).Posted);
        Assert.Equal(90, (await db.Set<CustTrans>().SingleAsync()).RecId);
        Assert.Equal(75m, (await db.Set<CustTransOpen>().SingleAsync()).AmountCur);
        Assert.Empty(await db.Set<CustSettlement>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }

    [Theory]
    [InlineData("BU-01", true, false)]
    [InlineData("", false, false)]
    [InlineData("BU-02", false, false)]
    [InlineData("BU-02", true, true)]
    public async Task Required_business_unit_is_validated_after_journal_defaults(
        string defaultValue, bool shouldPost, bool useLegacyEnteredValue)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new DimensionPostingContext(
            new DbContextOptionsBuilder<DimensionPostingContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        Seed(db);
        SeedSubledgers(db);
        db.AddRange(
            new DimensionAttribute { RecId = 1, DataAreaId = "dat", Name = "BusinessUnit" },
            new DimensionAttributeValue { RecId = 1, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "BU-01" },
            new DimensionAttributeValue { RecId = 2, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "BU-02" },
            new LedgerJournalTrans
            {
                RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
                AccountType = LedgerJournalACType.Cust, LedgerDimensionName = "C-001",
                OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
                AmountCurCredit = 300m
            });
        if (defaultValue.Length > 0)
        {
            db.Add(new DimensionAttributeValueSetItem
            {
                DataAreaId = "dat", DimensionAttributeValueSet = 1, DisplayValue = defaultValue,
                DimensionAttributeValue = defaultValue == "BU-01" ? 1 : 2
            });
            db.Set<LedgerJournalTable>().Local.Single().DefaultDimension = 1;
        }
        if (useLegacyEnteredValue)
        {
            foreach (var combination in db.Set<DimensionAttributeValueCombination>().Local.Where(x => x.RecId is 2 or 3))
                combination.BusinessUnitValue = "BU-01";
        }
        await db.SaveChangesAsync();
        var builder = new AccountStructureBuilder(db);
        await builder.DefineAsync("dat", new("BU", "Required business unit",
            [new(AccountStructureBuilder.MainAccountSegment, "*"), new("BusinessUnit", "BU-01")]), default);
        await builder.ActivateAsync("dat", "BU", default);
        await builder.AssignToLedgerAsync("dat", "BU", default);
        db.ChangeTracker.Clear();

        await using var transaction = await db.Database.BeginTransactionAsync();
        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var posting = new LedgerJournalPosting(db);
        if (!shouldPost)
        {
            var error = await Assert.ThrowsAsync<SubledgerPostingException>(() => posting.PostAsync("dat", header, default));
            Assert.Contains(defaultValue.Length == 0 ? "requires a BusinessUnit" : "'BU-02' is not an allowed BusinessUnit", error.Message);
            await transaction.RollbackAsync();
            db.ChangeTracker.Clear();
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
            Assert.Empty(await db.Set<CustTrans>().ToListAsync());
            Assert.Equal(NoYes.No, (await db.Set<LedgerJournalTable>().SingleAsync()).Posted);
            return;
        }

        await posting.PostAsync("dat", header, default);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(new[] { -300m, 300m }, entries.Select(x => x.AccountingCurrencyAmount).OrderBy(x => x));
        foreach (var entry in entries)
            Assert.Equal("BU-01", (await db.Set<DimensionAttributeValueCombination>().FindAsync(entry.LedgerDimension))!.BusinessUnitValue);
        Assert.Equal(-300m, (await db.Set<CustTrans>().SingleAsync()).AmountCur);
    }

    [Fact]
    public async Task A_customer_prepayment_is_not_silently_posted_as_an_ordinary_receivable()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.Add(new LedgerJournalTrans
        {
            RecId=1,DataAreaId="dat",JournalNum="J1",Voucher="V1",LineNum=1,
            TransDate=new DateTime(2026,1,15),CurrencyCode="SAR",AccountType=LedgerJournalACType.Cust,
            LedgerDimensionName="C-001",OffsetAccountType=LedgerJournalACType.Ledger,OffsetLedgerDimension=2,
            AmountCurCredit=300m,Prepayment=NoYes.Yes,Txt="Customer advance"
        });
        await db.SaveChangesAsync();
        var header=await db.Set<LedgerJournalTable>().SingleAsync();
        var error=await Assert.ThrowsAsync<SubledgerPostingException>(()=>new LedgerJournalPosting(db).PostAsync("dat",header,default));
        Assert.Contains("Customer prepayment",error.Message);
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());
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
    [InlineData("accountType", "a Cust account is required")]
    // A foreign transaction currency is stopped by the exchange-rate gate in the setup check,
    // before reaching the scope guard: either way the journal is refused, never approximated.
    [InlineData("currency", "no exchange rate type is configured")]
    [InlineData("reporting", "Reporting: no exchange rate type is configured")]
    [InlineData("tax", "journal tax calculation")]
    [InlineData("company", "intercompany")]
    [InlineData("offsetAccountType", "offset account must be a Ledger account")]
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
            case "reporting": db.Set<Ledger>().Local.Single().ReportingCurrency = "USD"; break;
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
        await ConfigureVoucherSequenceAsync(db);
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
        // Share capital is required to be a credit, and this journal debits it. The requirement is
        // DebitCreditCheck; DebitCreditProposal is only the usual side and restricts nothing.
        var shareCapital = await db.Set<MainAccount>().SingleAsync(x => x.RecId == 2);
        shareCapital.DebitCreditCheck = DebitCreditProposal.Credit;
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

    private static LedgerJournalTrans Line(long recId, string voucher, bool bank,
        decimal debit = 0m, decimal credit = 0m, DateTime? date = null) => new()
        {
            RecId = recId, DataAreaId = "dat", Company = "dat", JournalNum = "J1", LineNum = recId,
            Voucher = voucher, TransDate = date ?? new DateTime(2026, 1, 1), CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Ledger, LedgerDimension = bank ? 1 : 2,
            AmountCurDebit = debit, AmountCurCredit = credit,
            Txt = bank ? "Bank ledger control" : "Share capital"
        };

    // GL-04 of the reference cycle: with a reporting currency in use, every posted amount has to be
    // checked in the accounting *and* the reporting currency. The bank movement recorded the
    // accounting amount in its reporting field, so the two agreed only when the currencies did.
    [Fact]
    public async Task A_bank_line_records_the_reporting_amount_in_the_reporting_currency_field()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var date = new DateTime(2026, 1, 15);
        // SAR is the accounting currency, USD the reporting currency, at 0.25 USD per SAR.
        var ledger = db.Set<Ledger>().Local.Single();
        ledger.ReportingCurrency = "USD";
        ledger.DefaultExchangeRateType = 1;
        ledger.ReportingCurrencyExchangeRateType = 1;
        db.AddRange(
            new ExchangeRateType { RecId = 1, DataAreaId = "dat", IsActive = true },
            new ExchangeRateCurrencyPair { RecId = 1, DataAreaId = "dat", ExchangeRateType = 1,
                FromCurrencyCode = "SAR", ToCurrencyCode = "USD", ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One,
                IsActive = true },
            new ExchangeRate { RecId = 1, DataAreaId = "dat", ExchangeRateCurrencyPair = 1,
                ValidFrom = new DateTime(2026, 1, 1), ExchangeRateValue = 0.25m, IsActive = true });
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", Company = "dat", JournalNum = "J1", LineNum = 1,
            Voucher = "V1", TransDate = date, CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Bank, LedgerDimensionName = "BANK-1",
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
            AmountCurDebit = 2000m, Txt = "Customer collection"
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var movement = await db.Set<BankAccountTrans>().AsNoTracking().SingleAsync();
        Assert.Equal(2000m, movement.AmountCur);
        Assert.Equal(2000m, movement.AmountMst);
        // The whole point: the reporting field carries the reporting figure, not the accounting one.
        Assert.Equal(500m, movement.AmountReportingCurrency);
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

    [Theory]
    [InlineData("", "120100")]
    [InlineData("ALT", "120200")]
    public async Task A_customer_line_posts_the_summary_account_and_the_customer_transaction(string profile, string account)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        // Dr customer 500, Cr the ledger offset: the customer owes 500 more.
        db.AddRange(
            new MainAccount { RecId = 5, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "120200" },
            new DimensionAttributeValueCombination { RecId = 5, DataAreaId = "dat", MainAccount = 5, MainAccountValue = "120200", DisplayValue = "120200" },
            new CustLedgerAccounts { RecId = 2, DataAreaId = "dat", AccountCode = AccountCode.All, PostingProfile = "ALT", SummaryLedgerDimension = 5 });
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
            TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Cust, LedgerDimensionName = "C-001",
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
            AmountCurDebit = 500m, Txt = "Customer adjustment", PostingProfile = profile
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        await new LedgerJournalPosting(db).PostAsync("dat", header, default);
        await db.SaveChangesAsync();

        // The ledger hit the customer summary account, under the customer posting type.
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        var customerLine = lines.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        Assert.Equal(account, customerLine.LedgerAccount);
        Assert.Equal(500m, customerLine.AccountingCurrencyAmount);

        // The subledger movement exists, which is the whole point: the balance the ledger shows is
        // also on the customer's account, with an open row settlement can find.
        var custTrans = await db.Set<CustTrans>().AsNoTracking().SingleAsync();
        Assert.Equal("C-001", custTrans.AccountNum);
        Assert.Equal(profile, custTrans.PostingProfile);
        Assert.Equal(500m, custTrans.AmountCur);
        Assert.Equal(500m, custTrans.AmountMst);
        Assert.Equal("V1", custTrans.Voucher);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync();
        Assert.Equal(custTrans.RecId, open.RefRecId);
        Assert.Equal(500m, open.AmountCur);
    }

    [Fact]
    public async Task A_vendor_line_and_a_bank_line_post_their_own_subledger_movements()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        // Pay a vendor from the bank: Dr vendor 300 (what is owed falls), Cr bank 300.
        db.AddRange(
            new LedgerJournalTrans
            {
                RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
                AccountType = LedgerJournalACType.Vend, LedgerDimensionName = "V-001",
                AmountCurDebit = 300m, Txt = "Vendor settlement"
            },
            new LedgerJournalTrans
            {
                RecId = 2, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 2,
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
                AccountType = LedgerJournalACType.Bank, LedgerDimensionName = "BANK-1",
                AmountCurCredit = 300m, Txt = "Bank payment"
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        await new LedgerJournalPosting(db).PostAsync("dat", header, default);
        await db.SaveChangesAsync();

        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(300m, lines.Single(x => x.PostingType == LedgerPostingType.VendBalance).AccountingCurrencyAmount);
        Assert.Equal(-300m, lines.Single(x => x.PostingType == LedgerPostingType.Bank).AccountingCurrencyAmount);

        var vendTrans = await db.Set<VendTrans>().AsNoTracking().SingleAsync();
        Assert.Equal("V-001", vendTrans.AccountNum);
        Assert.Equal(300m, vendTrans.AmountCur);
        Assert.Equal(300m, (await db.Set<VendTransOpen>().AsNoTracking().SingleAsync()).AmountCur);

        var bankTrans = await db.Set<BankAccountTrans>().AsNoTracking().SingleAsync();
        Assert.Equal("BANK-1", bankTrans.AccountId);
        Assert.Equal(-300m, bankTrans.AmountCur);
        Assert.Equal(-300m, bankTrans.AmountMst);
    }

    [Fact]
    public async Task Account_types_outside_the_basic_set_and_a_subledger_offset_are_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.AddRange(
            // A fixed-asset line: outside the basic set.
            new LedgerJournalTrans
            {
                RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR",
                AccountType = LedgerJournalACType.FixedAssets, LedgerDimensionName = "FA-1",
                // Balanced by a ledger offset, so the account-type refusal is what surfaces rather
                // than the voucher balance check.
                OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
                AmountCurDebit = 100m
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        var fixedAsset = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", header, default));
        Assert.Contains("does not post through the basic general journal", fixedAsset.Message);

        // A customer line with no account named is refused rather than resolved to nothing.
        db.ChangeTracker.Clear();
        var line = await db.Set<LedgerJournalTrans>().SingleAsync();
        line.AccountType = LedgerJournalACType.Cust;
        line.LedgerDimensionName = string.Empty;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var reloaded = await db.Set<LedgerJournalTable>().SingleAsync();
        var missing = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", reloaded, default));
        Assert.Contains("account is required", missing.Message);

        // A subledger offset has no account field on the line, so it is refused by name.
        db.ChangeTracker.Clear();
        line = await db.Set<LedgerJournalTrans>().SingleAsync();
        line.AccountType = LedgerJournalACType.Ledger;
        line.LedgerDimension = 1;
        line.LedgerDimensionName = string.Empty;
        line.OffsetAccountType = LedgerJournalACType.Cust;
        line.OffsetLedgerDimension = 2;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var reloadedAgain = await db.Set<LedgerJournalTable>().SingleAsync();
        var offset = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", reloadedAgain, default));
        Assert.Contains("offset account must be a Ledger account", offset.Message);

        Assert.Empty(await db.Set<CustTrans>().AsNoTracking().ToListAsync());
    }

    /// <summary>
    /// Gives the fixture's journal name a usable voucher number sequence.
    /// <para>
    /// Reversal allocates a voucher for the reversing journal from the journal name's sequence and
    /// refuses by name when none is configured, which is correct: a reversing journal needs its own
    /// number. The fixture simply never configured one, so both reversal tests were failing on
    /// missing setup rather than on the behavior they exist to prove.
    /// </para>
    /// <para>
    /// Posting the original journal is unaffected: <c>AllocateForPostingAsync</c> returns early when
    /// every line already carries a voucher and the header does not allocate at posting, so the
    /// original still posts under its own "V1".
    /// </para>
    /// </summary>
    private static async Task ConfigureVoucherSequenceAsync(PostingContext db)
    {
        db.ChangeTracker.Clear();
        var journalName = await db.Set<LedgerJournalName>().SingleAsync();
        journalName.NumberSequenceTable = 90;
        db.Add(new SysNumberSequence
        {
            RecId = 90, DataAreaId = "dat", NumberSequence = "GEN-V", Txt = "General journal voucher",
            Format = "GV-######", AnnotatedFormat = "{PREFIX}-{SEQ}",
            Lowest = 100, NextRec = 100, Highest = 999,
            IsActive = true, InUse = 1, Manual = 0, NoIncrement = 0, Blocked = 0
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

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

    /// <summary>A customer, a vendor and a bank account, each with the account its lines post to.</summary>
    private static void SeedSubledgers(PostingContext db)
    {
        db.AddRange(
            new MainAccount { RecId = 3, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "120100" },
            new MainAccount { RecId = 4, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "210100" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "120100", DisplayValue = "120100" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, MainAccountValue = "210100", DisplayValue = "210100" },
            new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM", CurrencyCode = "SAR" },
            new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 3 },
            new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", VendGroup = "DOM" },
            new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", AccountCode = AccountCode.All, SummaryLedgerDimension = 4 },
            new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 1,
                BankAccountStatus = BankAccountStatus.Active, CurrencyCode = "SAR" });
    }

    private static async Task<PostingContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PostingContext(new DbContextOptionsBuilder<PostingContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class DimensionPostingContext(DbContextOptions<DimensionPostingContext> options) : PostingContext(options);

    private class PostingContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(LedgerJournalTable), typeof(LedgerJournalName), typeof(LedgerJournalTrans),
                typeof(SysNumberSequence),
                typeof(CompanyInfo), typeof(MainAccountLegalEntity), typeof(IAX.IXApi.Modules.Finance.Entities.LedgerParameters),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy),
                typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry),
                typeof(SubledgerVoucherGeneralJournalEntry), typeof(ExchangeRateType),
                typeof(ExchangeRateCurrencyPair), typeof(ExchangeRate),
                typeof(Currency),
                typeof(CustTable), typeof(CustTrans), typeof(CustTransOpen), typeof(CustParameters), typeof(CustSettlement),
                typeof(CustLedgerAccounts), typeof(VendTable), typeof(VendTrans), typeof(VendTransOpen),
                typeof(VendParameters), typeof(VendLedgerAccounts),
                typeof(BankAccountTable), typeof(BankAccountTrans)];
            if (this is DimensionPostingContext)
                types = [.. types, .. TestAccountStructures.Types.Except(types),
                    typeof(DimensionAttributeValueSetItem), typeof(DimensionAttributeValueGroup)];
            // Everything this harness does not list is ignored outright. Naming the strays one by
            // one does not hold: the customer and vendor tables reach the party, worker, group and
            // payment-term tables, and those reach further still.
            var included = new HashSet<Type>(types);
            foreach (var type in typeof(LedgerJournalTrans).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity<long>).IsAssignableFrom(t) && !included.Contains(t)))
                builder.Ignore(type);
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
        // Posting and reversal use the context directly.
        public IGenericRepository<T> Repository<T>() where T : class => null!;
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }
}
