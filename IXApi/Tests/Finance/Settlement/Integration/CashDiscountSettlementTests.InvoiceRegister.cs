using System.Reflection;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsPayable.InvoiceRegister;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// D365 invoice register and approval journals, matching the USMF voucher APIR000001: registering posts
/// Dr Offset (LedgerJournal) / Cr Arrival (VendBalance) and leaves the vendor transaction unapproved and unpayable;
/// approval posts Dr expense / Cr Offset (LedgerJournal) and Dr Arrival / Cr vendor summary (VendBalance) under the
/// same voucher, and the invoice can then be paid.
/// </summary>
public partial class CashDiscountSettlementTests
{
    private const long Arrival = 30, Offset = 31, Expense = 32;

    private static async Task SetUpInvoiceRegisterAsync(DbContext db)
    {
        db.AddRange(
            new MainAccount { RecId = 30, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "200130" },
            new MainAccount { RecId = 31, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "200125" },
            new MainAccount { RecId = 32, DataAreaId = "dat", LedgerChartOfAccounts = 10, MainAccountId = "605160" },
            new DimensionAttributeValueCombination { RecId = Arrival, DataAreaId = "dat", MainAccount = 30, MainAccountValue = "200130", DisplayValue = "200130" },
            new DimensionAttributeValueCombination { RecId = Offset, DataAreaId = "dat", MainAccount = 31, MainAccountValue = "200125", DisplayValue = "200125" },
            new DimensionAttributeValueCombination { RecId = Expense, DataAreaId = "dat", MainAccount = 32, MainAccountValue = "605160", DisplayValue = "605160" },
            new LedgerJournalName { RecId = 2, DataAreaId = "dat", JournalName = "APInvReg", JournalType = LedgerJournalType.PurchaseLedger, IsActive = true },
            new LedgerJournalName { RecId = 3, DataAreaId = "dat", JournalName = "APInvApp", JournalType = LedgerJournalType.Approval, IsActive = true });
        var rule = await db.Set<VendLedgerAccounts>().SingleAsync(x => x.RecId == 1);
        rule.PurchasingLedgerDimension = Arrival;
        rule.PurchasingOffsetLedgerDimension = Offset;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static VendInvoiceRegisterService NewRegister(DbContext db) =>
        new(new TestUnitOfWork(db), new TestUser(), DispatchProxy.Create<ISysNumberSequenceService, CashDiscountSequenceProxy>());

    private static async Task<Dictionary<long, decimal>> BalancesAsync(DbContext db) =>
        (await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync())
            .GroupBy(x => x.LedgerDimension).ToDictionary(x => x.Key, x => x.Sum(y => y.AccountingCurrencyAmount));

    [Fact]
    public async Task The_invoice_register_and_approval_post_as_D365_and_only_an_approved_invoice_is_paid()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SetUpInvoiceRegisterAsync(db);
        // Leave only the registered invoice open for the payments below.
        db.RemoveRange(await db.Set<VendTransOpen>().Where(x => x.RefRecId == 20).ToListAsync());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var voucher = await NewRegister(db).RegisterAsync(new VendInvoiceRegisterRequest(
            "APInvReg", "V-001", "123456", 706.23m, new DateOnly(2026, 1, 5)), default);

        var trans = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.Voucher == voucher);
        Assert.Equal(NoYes.No, trans.Approved);
        Assert.Equal(-706.23m, trans.AmountCur);
        var registered = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, registered.Count);
        Assert.Contains(registered, x => x.LedgerDimension == Offset && x.PostingType == LedgerPostingType.LedgerJournal && x.AccountingCurrencyAmount == 706.23m);
        Assert.Contains(registered, x => x.LedgerDimension == Arrival && x.PostingType == LedgerPostingType.VendBalance && x.AccountingCurrencyAmount == -706.23m);

        // An unapproved invoice is not paid: the payment stays on account.
        var early = await NewVendService(db).PostAsync(new VendPaymentPostDto("V-001", "BANK-1", 100m, "early", new DateTime(2026, 1, 6)), default);
        Assert.True(early.StatusCode == 200, early.Message);
        Assert.Empty(await db.Set<VendSettlement>().AsNoTracking().Where(x => x.InvoiceTransRecId == trans.RecId).ToListAsync());
        db.ChangeTracker.Clear();

        var approved = await NewRegister(db).ApproveAsync(new VendInvoiceApprovalRequest("APInvApp", voucher, Expense, new DateOnly(2026, 1, 7)), default);
        Assert.Equal(voucher, approved);
        Assert.Equal(NoYes.Yes, (await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == trans.RecId)).Approved);
        var balances = await BalancesAsync(db);
        Assert.Equal(706.23m, balances[Expense]);
        Assert.Equal(0m, balances[Offset]);
        Assert.Equal(0m, balances[Arrival]);
        Assert.Equal(-706.23m + 100m, balances[2]); // vendor summary: the invoice, less the earlier payment on account
        var journals = await db.Set<LedgerJournalTable>().AsNoTracking().Select(x => x.JournalType).ToListAsync();
        Assert.Equal(1, journals.Count(x => x == LedgerJournalType.PurchaseLedger));
        Assert.Equal(1, journals.Count(x => x == LedgerJournalType.Approval));

        db.ChangeTracker.Clear();
        var paid = await NewVendService(db).PostAsync(new VendPaymentPostDto("V-001", "BANK-1", 606.23m, "after", new DateTime(2026, 1, 8)), default);
        Assert.True(paid.StatusCode == 200, paid.Message);
        Assert.Equal(606.23m, (await db.Set<VendSettlement>().AsNoTracking()
            .Where(x => x.InvoiceTransRecId == trans.RecId).ToListAsync()).Sum(x => x.AmountCur));
    }

    [Fact]
    public async Task Approval_is_refused_twice_and_registration_needs_its_journal_name()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var missing = await Assert.ThrowsAnyAsync<Exception>(() => NewRegister(db).RegisterAsync(new VendInvoiceRegisterRequest(
            "APInvReg", "V-001", "X1", 10m, new DateOnly(2026, 1, 5)), default));
        Assert.Contains("APInvReg", missing.Message);
        db.ChangeTracker.Clear();
        await SetUpInvoiceRegisterAsync(db);

        var voucher = await NewRegister(db).RegisterAsync(new VendInvoiceRegisterRequest(
            "APInvReg", "V-001", "X1", 10m, new DateOnly(2026, 1, 5)), default);
        await NewRegister(db).ApproveAsync(new VendInvoiceApprovalRequest("APInvApp", voucher, Expense, new DateOnly(2026, 1, 5)), default);
        db.ChangeTracker.Clear();
        var again = await Assert.ThrowsAnyAsync<Exception>(() =>
            NewRegister(db).ApproveAsync(new VendInvoiceApprovalRequest("APInvApp", voucher, Expense, new DateOnly(2026, 1, 6)), default));
        Assert.Contains("unapproved", again.Message);
    }
}
