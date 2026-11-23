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
    public async Task Header_update_keeps_the_stored_tab_fields_a_client_did_not_send()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        await db.SaveChangesAsync();
        var stored = await db.Set<LedgerJournalTable>().SingleAsync(x => x.RecId == 1);
        stored.DetailSummaryPosting = DetailSummaryPosting.Summary;
        stored.LinesLimitBeforeDistribution = 1000;
        stored.ParentJournalNum = "J0";
        stored.Log = "Checked";
        stored.WorkflowApprovalStatus = WorkflowApprovalStatus.Approved;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // A journal page that only edits the description sends the other fields as defaults.
        var update = await db.Set<LedgerJournalTable>().SingleAsync(x => x.RecId == 1);
        update.Name = "Renamed";
        update.DetailSummaryPosting = DetailSummaryPosting.Detail;
        update.LinesLimitBeforeDistribution = 0;
        update.ParentJournalNum = "";
        update.Log = "";
        update.WorkflowApprovalStatus = WorkflowApprovalStatus.NotSubmitted;
        // The service restores the stored fields before the generic repository save; this test
        // harness has no repository, so that save step fails and the prepared entity is checked.
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).UpdateAsync(update));
        var saved = update;
        Assert.Equal("Renamed", saved.Name);
        Assert.Equal(DetailSummaryPosting.Summary, saved.DetailSummaryPosting);
        Assert.Equal(1000, saved.LinesLimitBeforeDistribution);
        Assert.Equal("J0", saved.ParentJournalNum);
        Assert.Equal("Checked", saved.Log);
        Assert.Equal(WorkflowApprovalStatus.Approved, saved.WorkflowApprovalStatus);
    }

    [Fact]
    public async Task Line_update_keeps_the_stored_tab_fields_a_client_did_not_send()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.Add(new LedgerJournalTrans { RecId = 900, DataAreaId = "dat", JournalNum = "J1", LineNum = 1, Txt = "Payment" });
        await db.SaveChangesAsync();
        var stored = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.RecId == 900);
        stored.PaymSpec = "SPEC";
        stored.BankTransType = "01";
        stored.Qty = 2;
        stored.Transferred = NoYes.Yes;
        stored.TransferredBy = "batch";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var update = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.RecId == 900);
        update.Txt = "Renamed";
        update.PaymSpec = "";
        update.BankTransType = "";
        update.Qty = 0;
        update.Transferred = NoYes.No;
        update.TransferredBy = "";
        // The service restores the stored fields before the generic repository save; this test
        // harness has no repository, so that save step fails and the prepared entity is checked.
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            new LedgerJournalTransService(new TestUnitOfWork(db), new TestUser()).UpdateAsync(update));

        Assert.Equal("Renamed", update.Txt);
        Assert.Equal("SPEC", update.PaymSpec);
        Assert.Equal("01", update.BankTransType);
        Assert.Equal(2, update.Qty);
        Assert.Equal(NoYes.Yes, update.Transferred);
        Assert.Equal("batch", update.TransferredBy);
    }

    [Fact]
    public async Task Offset_lookup_returns_company_scoped_ledger_and_bank_dimension_ids()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.AddRange(
            new DimensionAttributeValueCombination { RecId = 10, DataAreaId = "dat", DisplayValue = "BANK-1" },
            new DimensionAttributeValueCombination { RecId = 11, DataAreaId = "dat", DisplayValue = "C-001" },
            new DimensionAttributeValueCombination { RecId = 12, DataAreaId = "other", MainAccount = 1, DisplayValue = "OTHER" },
            new DimensionAttributeValueCombination { RecId = 13, DataAreaId = "dat", MainAccount = 1, DisplayValue = "DELETED", IsDeleted = true });
        await db.SaveChangesAsync();
        var rows = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).OffsetAccountsAsync(default);
        Assert.Contains(rows, row => row.Id == 1 && row.AccountType == 0);
        Assert.Contains(rows, row => row.Id == 10 && row.AccountType == 5 && row.Code == "BANK-1");
        Assert.DoesNotContain(rows, row => row.Id is 11 or 12 or 13);
    }

    [Theory]
    [InlineData("other-journal")]
    [InlineData("account")]
    public async Task Posting_does_not_commit_unrelated_pending_changes(string pending)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        if (pending == "other-journal")
        {
            var other = Line(3, "V2", bank: true, debit: 50m);
            other.JournalNum = "J2";
            db.Add(other);
        }
        else
            (await db.Set<MainAccount>().SingleAsync(x => x.RecId == 1)).Name = "Unsaved account change";

        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);

        Assert.Null(result);
        Assert.Contains("other pending changes", error);
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
        Assert.Equal(2, await db.Set<LedgerJournalTrans>().AsNoTracking().CountAsync());
        Assert.NotEqual("Unsaved account change", (await db.Set<MainAccount>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Name);
    }

    [Fact]
    public async Task Posting_refuses_unsaved_header_changes_instead_of_discarding_them()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        header.Name = "Unsaved description";

        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);

        Assert.Null(result);
        Assert.Contains("Save journal header changes", error);
        Assert.Empty(await db.Set<GeneralJournalEntry>().AsNoTracking().ToListAsync());
        Assert.NotEqual("Unsaved description", (await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync()).Name);
    }

    [Fact]
    public async Task Posting_refreshes_tracked_lines_to_match_the_persisted_journal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        await db.Set<LedgerJournalTrans>().Where(x => x.RecId == 1).ExecuteUpdateAsync(s => s.SetProperty(x => x.AmountCurDebit, 200m));
        await db.Set<LedgerJournalTrans>().Where(x => x.RecId == 2).ExecuteUpdateAsync(s => s.SetProperty(x => x.AmountCurCredit, 200m));
        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(200m, result.TotalDebit);
        Assert.Equal(200m, result.TotalCredit);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(200m, entries.Single(x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        Assert.Equal(-200m, entries.Single(x => x.LedgerDimension == 2).AccountingCurrencyAmount);
    }

    [Theory]
    [InlineData("add")]
    [InlineData("add-blank-company")]
    [InlineData("edit")]
    [InlineData("edit-away-journal")]
    [InlineData("edit-away-company")]
    [InlineData("delete")]
    public async Task Posting_refuses_unsaved_line_changes(string operation)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        var line = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.RecId == 1);
        if (operation == "add") db.Add(Line(3, "V2", bank: true, debit: 50m));
        else if (operation == "add-blank-company")
        {
            var added = Line(3, "V2", bank: true, debit: 50m);
            added.DataAreaId = "";
            db.Add(added);
        }
        else if (operation == "edit") line.AmountCurDebit = 200m;
        else if (operation == "edit-away-journal") line.JournalNum = "J2";
        else if (operation == "edit-away-company") line.DataAreaId = "other";
        else db.Remove(line);
        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);
        Assert.Null(result);
        Assert.Contains("Save journal line changes", error);
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Equal(2, await db.Set<LedgerJournalTrans>().CountAsync());
        Assert.Equal(100m, (await db.Set<LedgerJournalTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1)).AmountCurDebit);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_stale_marked_balance_rolls_back_journal_posting(bool cachedBalance)
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
            AmountCur = cachedBalance ? 100m : 75m, AmountMst = cachedBalance ? 100m : 75m,
            ReportingCurrencyAmount = cachedBalance ? 100m : 75m });
        db.Add(new LedgerJournalTrans
        {
            RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
            TransDate = date, CurrencyCode = "SAR", AccountType = LedgerJournalACType.Cust,
            LedgerDimensionName = "C-001", OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = 2,
            AmountCurCredit = 100m, MarkedInvoiceRecId = 90, SettleVoucher = SettleVoucher.Selection
        });
        await db.SaveChangesAsync();
        if (cachedBalance)
        {
            // Another operation reduced the persisted balance while this context retained 100.
            await db.Set<CustTransOpen>().ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.AmountCur, 75m).SetProperty(x => x.AmountMst, 75m)
                .SetProperty(x => x.ReportingCurrencyAmount, 75m));
        }
        else db.ChangeTracker.Clear();

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

    [Fact]
    public async Task Posting_refreshes_header_controls_changed_since_it_was_loaded()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        await db.Set<LedgerJournalTable>().ExecuteUpdateAsync(s => s.SetProperty(x => x.ReverseEntry, NoYes.Yes));

        var error = await Assert.ThrowsAsync<SubledgerPostingException>(
            () => new LedgerJournalPosting(db).PostAsync("dat", header, default));
        Assert.Contains("Automatic reversal", error.Message);
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }


    private static LedgerJournalReversalService NewReversalService(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>());


    /// <summary>
    /// The account combinations an offset customer, vendor or bank account is named through: the
    /// display value is the account number and there is no main account, because that comes from
    /// the posting setup of the account named.
    /// </summary>
    private static void SeedOffsetAccounts(PostingContext db) => db.AddRange(
        new DimensionAttributeValueCombination { RecId = 10, DataAreaId = "dat", DisplayValue = "C-001" },
        new DimensionAttributeValueCombination { RecId = 11, DataAreaId = "dat", DisplayValue = "V-001" },
        new DimensionAttributeValueCombination { RecId = 12, DataAreaId = "dat", DisplayValue = "BANK-1" });

    private static LedgerJournalTrans Line(long recId, string voucher, bool bank,
        decimal debit = 0m, decimal credit = 0m, DateTime? date = null) => new()
        {
            RecId = recId, DataAreaId = "dat", Company = "dat", JournalNum = "J1", LineNum = recId,
            Voucher = voucher, TransDate = date ?? new DateTime(2026, 1, 1), CurrencyCode = "SAR",
            AccountType = LedgerJournalACType.Ledger, LedgerDimension = bank ? 1 : 2,
            AmountCurDebit = debit, AmountCurCredit = credit,
            Txt = bank ? "Bank ledger control" : "Share capital"
        };

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

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Bulk_mutations_cannot_change_posted_journals(bool lines, bool delete)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        var header = await db.Set<LedgerJournalTable>().SingleAsync();
        await new LedgerJournalPosting(db).PostAsync("dat", header, default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var work = new TestUnitOfWork(db);
        if (lines)
        {
            var service = new LedgerJournalTransService(work, new TestUser());
            var row = await db.Set<LedgerJournalTrans>().AsNoTracking().FirstAsync();
            row.AmountCurDebit = 999m;
            if (delete) await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveRangeAsync([row]));
            else await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateRangeAsync([row]));
        }
        else
        {
            var service = new LedgerJournalTableService(work, new TestUser());
            var row = await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync();
            row.Posted = NoYes.No;
            if (delete) await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveRangeAsync([row]));
            else await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateRangeAsync([row]));
        }
        Assert.Equal(NoYes.Yes, (await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync()).Posted);
        Assert.Equal(2, await db.Set<LedgerJournalTrans>().CountAsync());
        Assert.Equal(100m, (await db.Set<LedgerJournalTrans>().AsNoTracking().SingleAsync(x => x.RecId == 1)).AmountCurDebit);
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
                typeof(CustLedgerAccounts), typeof(VendTable), typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(VendInvoiceJour),
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
