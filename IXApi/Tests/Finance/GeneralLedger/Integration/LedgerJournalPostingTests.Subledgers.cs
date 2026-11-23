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
    public async Task Bank_movement_uses_the_same_defaulted_dimension_as_the_posted_ledger_entry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new DimensionPostingContext(
            new DbContextOptionsBuilder<DimensionPostingContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        Seed(db);
        SeedSubledgers(db);
        db.AddRange(new DimensionAttribute { RecId = 1, DataAreaId = "dat", Name = "BusinessUnit" },
            new DimensionAttributeValue { RecId = 1, DataAreaId = "dat", DimensionAttribute = 1, DisplayValue = "BU-01" },
            new DimensionAttributeValueSetItem { DataAreaId = "dat", DimensionAttributeValueSet = 1,
                DisplayValue = "BU-01", DimensionAttributeValue = 1 },
            new LedgerJournalTrans { RecId = 1, DataAreaId = "dat", JournalNum = "J1", Voucher = "V1", LineNum = 1,
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR", AccountType = LedgerJournalACType.Bank,
                LedgerDimensionName = "BANK-1", OffsetAccountType = LedgerJournalACType.Ledger,
                OffsetLedgerDimension = 2, AmountCurDebit = 100m });
        db.Set<LedgerJournalTable>().Local.Single().DefaultDimension = 1;
        await db.SaveChangesAsync();
        var builder = new AccountStructureBuilder(db);
        await builder.DefineAsync("dat", new("BU", "Required business unit",
            [new(AccountStructureBuilder.MainAccountSegment, "*"), new("BusinessUnit", "BU-01")]), default);
        await builder.ActivateAsync("dat", "BU", default);
        await builder.AssignToLedgerAsync("dat", "BU", default);
        db.ChangeTracker.Clear();

        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);

        Assert.Null(error);
        Assert.NotNull(result);
        var posted = await db.Set<GeneralJournalAccountEntry>().SingleAsync(x => x.PostingType == LedgerPostingType.Bank);
        var movement = await db.Set<BankAccountTrans>().SingleAsync();
        Assert.NotEqual(1, posted.LedgerDimension);
        Assert.Equal(posted.LedgerDimension, movement.LedgerDimension);
    }

    [Fact]
    public async Task A_customer_journal_invoice_preserves_metadata_and_can_be_selected_for_settlement()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var invoiceLine = Line(1, "V1", bank: true, debit: 100m);
        invoiceLine.AccountType = LedgerJournalACType.Cust;
        invoiceLine.LedgerDimensionName = "C-001";
        invoiceLine.OffsetLedgerDimension = 2;
        invoiceLine.Invoice = "INV-J1";
        invoiceLine.DocumentDate = new DateTime(2026, 1, 10);
        invoiceLine.PaymReference = "REF-J1";
        db.Add(invoiceLine);
        await db.SaveChangesAsync();
        var service = new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser());
        var (invoiceResult, invoiceError) = await service.PostAsync(1, default);
        Assert.Null(invoiceError);
        Assert.NotNull(invoiceResult);
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync();
        Assert.Equal("INV-J1", invoice.Invoice);
        Assert.Equal(invoiceLine.DocumentDate, invoice.DocumentDate);
        Assert.Equal("REF-J1", invoice.PaymReference);

        db.Add(new LedgerJournalTable { RecId = 2, DataAreaId = "dat", JournalNum = "J2", JournalName = "GEN" });
        var credit = Line(2, "V2", bank: true, credit: 100m);
        credit.JournalNum = "J2";
        credit.AccountType = LedgerJournalACType.Cust;
        credit.LedgerDimensionName = "C-001";
        credit.OffsetLedgerDimension = 2;
        credit.SettleVoucher = SettleVoucher.Selection;
        credit.MarkedInvoiceRecId = invoice.RecId;
        db.Add(credit);
        await db.SaveChangesAsync();
        var (result, error) = await service.PostAsync(2, default);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        Assert.Equal(2, await db.Set<CustSettlement>().CountAsync());
    }

    [Fact]
    public async Task Vendor_payment_journal_settles_the_marked_vendor_invoice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var invoiceLine = Line(1, "V1", bank: true, credit: 100m);
        invoiceLine.AccountType = LedgerJournalACType.Vend;
        invoiceLine.LedgerDimensionName = "V-001";
        invoiceLine.OffsetLedgerDimension = 2;
        invoiceLine.Invoice = "VINV-1";
        db.Add(invoiceLine);
        await db.SaveChangesAsync();
        var service = new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser());
        var (invoiceResult, invoiceError) = await service.PostAsync(1, default);
        Assert.Null(invoiceError);
        Assert.NotNull(invoiceResult);
        var invoice = await db.Set<VendTrans>().AsNoTracking().SingleAsync();
        Assert.Equal(-100m, invoice.AmountCur);

        db.Add(new LedgerJournalTable { RecId = 2, DataAreaId = "dat", JournalNum = "J2", JournalName = "GEN" });
        var payment = Line(2, "V2", bank: true, debit: 100m);
        payment.JournalNum = "J2";
        payment.AccountType = LedgerJournalACType.Vend;
        payment.LedgerDimensionName = "V-001";
        payment.OffsetLedgerDimension = 2;
        payment.SettleVoucher = SettleVoucher.Selection;
        payment.MarkedInvoiceRecId = invoice.RecId;
        db.Add(payment);
        await db.SaveChangesAsync();
        var (result, error) = await service.PostAsync(2, default);
        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Empty(await db.Set<VendTransOpen>().ToListAsync());
        var settlement = Assert.Single(await db.Set<VendSettlement>().ToListAsync());
        Assert.Equal(invoice.RecId, settlement.InvoiceTransRecId);
        Assert.Equal(100m, settlement.AmountCur);
        Assert.All(await db.Set<VendTrans>().AsNoTracking().ToListAsync(), trans => Assert.NotEqual(default, trans.Closed));
    }

    [Fact]
    public async Task Vendor_payment_marking_requires_a_matching_open_invoice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        var payment = Line(1, "V1", bank: true, debit: 100m);
        payment.AccountType = LedgerJournalACType.Vend;
        payment.LedgerDimensionName = "V-001";
        payment.OffsetLedgerDimension = 2;
        payment.SettleVoucher = SettleVoucher.Selection;
        payment.MarkedInvoiceRecId = 999;
        db.Add(payment);
        await db.SaveChangesAsync();
        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);
        Assert.Null(result);
        Assert.Contains("marked invoice", error);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
        Assert.Empty(await db.Set<VendSettlement>().ToListAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Customer_invoicing_hold_blocks_invoice_journals_but_allows_unmarked_credits(bool invoice)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.Set<CustTable>().Local.Single().Blocked = CustVendorBlocked.Invoicing;
        var line = Line(1, "V1", bank: true, credit: 100m);
        line.AccountType = LedgerJournalACType.Cust;
        line.LedgerDimensionName = "C-001";
        line.OffsetLedgerDimension = 2;
        line.Invoice = invoice ? "INV-J1" : "";
        db.Add(line);
        await db.SaveChangesAsync();
        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);
        if (invoice)
        {
            Assert.Null(result);
            Assert.Contains("customer is blocked", error);
            Assert.Empty(await db.Set<CustTrans>().ToListAsync());
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        }
        else
        {
            Assert.Null(error);
            Assert.NotNull(result);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Customer_journals_respect_the_all_transactions_block(bool blocked, bool debit)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        db.Set<CustTable>().Local.Single().Blocked = blocked ? CustVendorBlocked.All : CustVendorBlocked.No;
        var line = Line(1, "V1", bank: true, debit: debit ? 100m : 0m, credit: debit ? 0m : 100m);
        line.AccountType = LedgerJournalACType.Cust;
        line.LedgerDimensionName = "C-001";
        line.LedgerDimension = 0;
        line.OffsetLedgerDimension = 2;
        db.Add(line);
        await db.SaveChangesAsync();
        var (result, error) = await new LedgerJournalTableService(new TestUnitOfWork(db), new TestUser()).PostAsync(1, default);
        if (blocked)
        {
            Assert.Null(result);
            Assert.Contains("customer is blocked", error);
            Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
            Assert.Empty(await db.Set<CustTrans>().ToListAsync());
            Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
            Assert.Equal(NoYes.No, (await db.Set<LedgerJournalTable>().SingleAsync()).Posted);
        }
        else
        {
            Assert.Null(error);
            Assert.NotNull(result);
            var open = await db.Set<CustTransOpen>().SingleAsync();
            var transaction = await db.Set<CustTrans>().SingleAsync();
            var postedLine = await db.Set<LedgerJournalTrans>().AsNoTracking().SingleAsync();
            Assert.Equal(debit ? 100m : -100m, open.AmountCur);
            Assert.Equal(transaction.RecId, open.RefRecId);
            Assert.Equal(transaction.RecId, postedLine.CustTransId);
        }
    }

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
        Assert.Contains("Save journal header changes", errorBeforeApproval.Message);
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Equal(100m, (await db.Set<CustTransOpen>().SingleAsync()).AmountCur);

        // Save the approval before retrying; an unsaved approval must not authorize posting.
        header.WorkflowApprovalStatus = WorkflowApprovalStatus.Approved;
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

    // An offset account may be any of the types the account side accepts, so one line can move an
    // amount between a ledger account and a customer, vendor or bank account. The offset side owes
    // its subledger movement exactly as the account side does.
    [Theory]
    [InlineData(LedgerJournalACType.Cust, 10L, 3L, LedgerPostingType.CustBalance)]
    [InlineData(LedgerJournalACType.Vend, 11L, 4L, LedgerPostingType.VendBalance)]
    [InlineData(LedgerJournalACType.Bank, 12L, 1L, LedgerPostingType.Bank)]
    public async Task A_subledger_offset_posts_its_ledger_entry_and_its_subledger_movement(
        LedgerJournalACType offsetType, long offsetAccount, long summaryDimension, LedgerPostingType postingType)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        Seed(db);
        SeedSubledgers(db);
        SeedOffsetAccounts(db);
        var line = Line(1, "V1", bank: false, debit: 100m);
        line.OffsetAccountType = offsetType;
        line.OffsetLedgerDimension = offsetAccount;
        line.OffsetTxt = "Offset side";
        line.Invoice = "INV-ACCOUNT-SIDE";
        db.Add(line);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new LedgerJournalPosting(db).PostAsync("dat", await db.Set<LedgerJournalTable>().SingleAsync(), default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(1, result.VouchersPosted);
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        var offset = Assert.Single(entries, x => x.PostingType == postingType);
        Assert.Equal(summaryDimension, offset.LedgerDimension);
        Assert.Equal(-100m, offset.AccountingCurrencyAmount);

        // The line still names the offset account; the summary account is derived, not stored.
        var posted = await db.Set<LedgerJournalTrans>().AsNoTracking().SingleAsync();
        Assert.Equal(offsetAccount, posted.OffsetLedgerDimension);
        Assert.Equal(0, posted.CustTransId);
        Assert.Equal(0, posted.VendTransId);

        switch (offsetType)
        {
            case LedgerJournalACType.Cust:
                var customer = await db.Set<CustTrans>().AsNoTracking().SingleAsync();
                Assert.Equal("C-001", customer.AccountNum);
                Assert.Equal(-100m, customer.AmountCur);
                Assert.Equal("Offset side", customer.Txt);
                Assert.Equal(string.Empty, customer.Invoice); // the invoice describes the account side
                Assert.Equal(-100m, (await db.Set<CustTransOpen>().AsNoTracking().SingleAsync()).AmountCur);
                break;
            case LedgerJournalACType.Vend:
                var vendor = await db.Set<VendTrans>().AsNoTracking().SingleAsync();
                Assert.Equal("V-001", vendor.AccountNum);
                Assert.Equal(-100m, vendor.AmountCur);
                Assert.Equal(string.Empty, vendor.Invoice);
                break;
            default:
                var bank = await db.Set<BankAccountTrans>().AsNoTracking().SingleAsync();
                Assert.Equal("BANK-1", bank.AccountId);
                Assert.Equal(-100m, bank.AmountCur);
                break;
        }
    }

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

        // A customer offset pointing at a main account combination names no customer, so it is refused.
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
        Assert.Contains("must reference a Cust account, not a main account combination", offset.Message);

        Assert.Empty(await db.Set<CustTrans>().AsNoTracking().ToListAsync());
    }
}
