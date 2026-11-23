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
    [InlineData("ledger")]
    [InlineData("customer")]
    [InlineData("vendor-exclusive")]
    [InlineData("vendor-group-only")]
    [InlineData("vendor-item-only")]
    [InlineData("vendor-no-offset")]
    public async Task Unsupported_tax_groups_are_refused_instead_of_posting_without_tax(string scenario)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var line = Line(1, "V1", bank: true, debit: 100m);
        line.OffsetLedgerDimension = 2;
        line.TaxGroup = "VAT";
        line.TaxItemGroup = "VAT15";
        if (scenario != "ledger")
        {
            line.AccountType = scenario == "customer" ? LedgerJournalACType.Cust : LedgerJournalACType.Vend;
            line.LedgerDimensionName = scenario == "customer" ? "C-001" : "V-001";
            line.LedgerDimension = 0;
        }
        if (scenario == "vendor-group-only") line.TaxItemGroup = "";
        if (scenario == "vendor-item-only") line.TaxGroup = "";
        if (scenario == "vendor-no-offset")
        {
            db.Set<LedgerJournalTable>().Local.Single().LedgerJournalInclTax = NoYes.Yes;
            line.OffsetLedgerDimension = 0;
            db.Add(Line(2, "V1", bank: false, credit: 100m));
        }
        db.Add(line);
        await db.SaveChangesAsync();
        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);
        Assert.Null(result);
        Assert.Contains("journal tax calculation is not supported", error);
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
    }

    [Theory]
    [InlineData(LedgerJournalACType.Cust, SettleVoucher.Automatic)]
    [InlineData(LedgerJournalACType.Vend, SettleVoucher.Automatic)]
    [InlineData(LedgerJournalACType.Ledger, SettleVoucher.Selection)]
    public async Task Unsupported_journal_settlement_requests_are_not_silently_ignored(LedgerJournalACType accountType, SettleVoucher settlement)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var line = Line(1, "V1", bank: true, debit: 100m);
        line.AccountType = accountType;
        line.LedgerDimensionName = accountType == LedgerJournalACType.Cust ? "C-001" : "V-001";
        line.OffsetLedgerDimension = 2;
        line.SettleVoucher = settlement;
        db.Add(line);
        await db.SaveChangesAsync();
        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);
        Assert.Null(result);
        Assert.Contains("use the settlement process", error);
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Posting_honors_the_fixed_offset_from_header_or_journal_name(bool fixedOnName, bool matchingOffset)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        if (fixedOnName)
        {
            var name = db.Set<LedgerJournalName>().Local.Single();
            name.FixedOffsetAccount = NoYes.Yes;
            name.OffsetLedgerDimension = 2;
        }
        else
        {
            var header = db.Set<LedgerJournalTable>().Local.Single();
            header.FixedOffsetAccount = NoYes.Yes;
            header.OffsetLedgerDimension = 2;
        }
        var line = Line(1, "V1", bank: true, debit: 100m);
        line.OffsetLedgerDimension = matchingOffset ? 2 : 1;
        db.Add(line);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser());
        var validation = await service.ValidateAsync(1, default);
        Assert.Equal(matchingOffset, validation!.Valid);
        var (result, error) = await service.PostAsync(1, default);
        if (matchingOffset)
        {
            Assert.Null(error);
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
            Assert.Contains("fixed offset account", error);
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        }
    }

    [Theory]
    [InlineData("line-accounting")]
    [InlineData("line-reporting")]
    [InlineData("header-accounting")]
    [InlineData("header-reporting")]
    [InlineData("header-fixed")]
    [InlineData("name-fixed")]
    public async Task Posting_refuses_exchange_rate_controls_it_cannot_apply(string control)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        var line = Line(1, "V1", bank: true, debit: 100m);
        line.OffsetLedgerDimension = 2;
        var header = db.Set<LedgerJournalTable>().Local.Single();
        switch (control)
        {
            case "line-accounting": line.ExchRate = 375m; break;
            case "line-reporting": line.ReportingCurrencyExchRate = 90m; break;
            case "header-accounting": header.ExchRate = 375m; break;
            case "header-reporting": header.ReportingCurrencyExchRate = 90m; break;
            case "header-fixed": header.FixedExchRate = NoYes.Yes; break;
            case "name-fixed": db.Set<LedgerJournalName>().Local.Single().FixedExchRate = NoYes.Yes; break;
        }
        db.Add(line);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser());
        var validation = await service.ValidateAsync(1, default);
        Assert.False(validation!.Valid);
        var (result, error) = await service.PostAsync(1, default);
        Assert.Null(result);
        Assert.Contains("exchange rates are not supported", error);
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
    public async Task Missing_ledger_is_a_posting_refusal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        db.AddRange(Line(1, "V1", bank: true, debit: 100m), Line(2, "V1", bank: false, credit: 100m));
        await db.SaveChangesAsync();
        await db.Set<Ledger>().ExecuteDeleteAsync();
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() =>
            new LedgerJournalPosting(db).PostAsync("dat", db.Set<LedgerJournalTable>().Local.Single(), default));
        Assert.Contains("Exactly one ledger", error.Message);
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }

    [Theory]
    [InlineData("accountType", "a Cust account is required")]
    // A foreign transaction currency is stopped by the exchange-rate gate in the setup check,
    // before reaching the scope guard: either way the journal is refused, never approximated.
    [InlineData("currency", "no exchange rate type is configured")]
    [InlineData("reporting", "Reporting: no exchange rate type is configured")]
    [InlineData("tax", "journal tax calculation")]
    [InlineData("company", "intercompany")]
    [InlineData("offsetAccountType", "must reference a Vend account, not a main account combination")]
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
            // A vendor offset must name a vendor; a main account combination is refused, not guessed at.
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
}
