using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// An invoice on a payment schedule has one open row per installment (D365 CustTransOpen per due date). Payments
/// settle the earliest installment first, the transaction closes only with its last installment, and reversing a
/// settlement gives each amount back to the installment it came from.
/// </summary>
public partial class CashDiscountSettlementTests
{
    private static readonly DateTime[] InstallmentDue = [new(2026, 1, 31), new(2026, 2, 28), new(2026, 3, 31)];

    private static async Task SplitIntoInstallmentsAsync(DbContext db)
    {
        var invoice = await db.Set<CustTrans>().SingleAsync(x => x.RecId == 10);
        invoice.CashDiscCode = string.Empty;
        db.Remove(await db.Set<CustTransOpen>().SingleAsync(x => x.RefRecId == 10));
        decimal[] amounts = [400m, 300m, 300m];
        for (var i = 0; i < 3; i++)
            db.Add(new CustTransOpen
            {
                RecId = 100 + i, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 10, TransDate = new DateTime(2026, 1, 1),
                DueDate = InstallmentDue[i], AmountCur = amounts[i], AmountMst = amounts[i], ReportingCurrencyAmount = amounts[i]
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task A_payment_settles_installments_by_due_date_and_the_invoice_closes_with_the_last()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SplitIntoInstallmentsAsync(db);

        var first = await NewService(db).PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 500m, new DateTime(2026, 1, 20)), default);
        Assert.True(first.StatusCode == 200, first.Message);
        var open = await db.Set<CustTransOpen>().AsNoTracking().Where(x => x.RefRecId == 10).OrderBy(x => x.DueDate).ToListAsync();
        Assert.Equal(new[] { 200m, 300m }, open.Select(x => x.AmountCur));
        Assert.Equal(new[] { InstallmentDue[1], InstallmentDue[2] }, open.Select(x => x.DueDate));
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.NotEqual(CustSettlementStatus.Settled, invoice.Settlement);
        var rows = await db.Set<CustSettlement>().AsNoTracking().Where(x => x.TransRecId == 10).OrderBy(x => x.DueDate).ToListAsync();
        Assert.Equal(new[] { 400m, 100m }, rows.Select(x => x.SettleAmountCur));
        Assert.Equal(new[] { InstallmentDue[0], InstallmentDue[1] }, rows.Select(x => x.DueDate));

        db.ChangeTracker.Clear();
        var second = await NewService(db).PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 500m, new DateTime(2026, 1, 25)), default);
        Assert.True(second.StatusCode == 200, second.Message);
        Assert.Empty(await db.Set<CustTransOpen>().AsNoTracking().Where(x => x.RefRecId == 10).ToListAsync());
        invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(CustSettlementStatus.Settled, invoice.Settlement);
        Assert.Equal(1000m, invoice.SettleAmountCur);
    }

    [Fact]
    public async Task Reversing_a_settlement_gives_each_amount_back_to_its_own_installment()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SplitIntoInstallmentsAsync(db);
        var paid = await NewService(db).PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 500m, new DateTime(2026, 1, 20)), default);
        Assert.True(paid.StatusCode == 200, paid.Message);
        db.ChangeTracker.Clear();

        var voucher = (await db.Set<CustSettlement>().AsNoTracking().FirstAsync(x => x.TransRecId == 10)).SettlementVoucher;
        Assert.Null(await CustSettlementRestore.ApplyAsync(db, "dat", voucher, new DateTime(2026, 1, 21), "REV-1", default));
        await db.SaveChangesAsync();

        var open = await db.Set<CustTransOpen>().AsNoTracking().Where(x => x.RefRecId == 10).OrderBy(x => x.DueDate).ToListAsync();
        Assert.Equal(new[] { 400m, 300m, 300m }, open.Select(x => x.AmountCur));
        Assert.Equal(InstallmentDue, open.Select(x => x.DueDate));
        Assert.Equal(new[] { 400m, 300m, 300m }, open.Select(x => x.AmountMst));
        var invoice = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.RecId == 10);
        Assert.Equal(0m, invoice.SettleAmountCur);
    }

    private static async Task SplitVendorInvoiceIntoInstallmentsAsync(DbContext db)
    {
        var invoice = await db.Set<VendTrans>().SingleAsync(x => x.RecId == 20);
        invoice.CashDiscCode = string.Empty;
        db.Remove(await db.Set<VendTransOpen>().SingleAsync(x => x.RefRecId == 20));
        decimal[] amounts = [400m, 300m, 300m];
        for (var i = 0; i < 3; i++)
            db.Add(new VendTransOpen
            {
                RecId = 200 + i, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 20, TransDate = new DateTime(2026, 1, 1),
                DueDate = InstallmentDue[i], AmountCur = -amounts[i], AmountMst = -amounts[i]
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task A_vendor_payment_settles_installments_by_due_date_and_its_reversal_restores_each_installment()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await SplitVendorInvoiceIntoInstallmentsAsync(db);

        var paid = await NewVendService(db).PostAsync(
            new VendPaymentPostDto("V-001", "BANK-1", 500m, "req-inst", new DateTime(2026, 1, 20)), default);
        Assert.True(paid.StatusCode == 200, paid.Message);
        var open = await db.Set<VendTransOpen>().AsNoTracking().Where(x => x.RefRecId == 20).OrderBy(x => x.DueDate).ToListAsync();
        Assert.Equal(new[] { -200m, -300m }, open.Select(x => x.AmountCur));
        var rows = await db.Set<VendSettlement>().AsNoTracking().Where(x => x.InvoiceTransRecId == 20).OrderBy(x => x.DueDate).ToListAsync();
        Assert.Equal(new[] { 400m, 100m }, rows.Select(x => x.AmountCur));
        Assert.Equal(new[] { InstallmentDue[0], InstallmentDue[1] }, rows.Select(x => x.DueDate));
        Assert.Equal(new long[] { 200, 201 }, rows.Select(x => x.TransOpen));
        Assert.Equal(default, (await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 20)).Closed);
        db.ChangeTracker.Clear();

        Assert.Null(await VendSettlementRestore.ApplyAsync(db, "dat", rows[0].Voucher, new DateTime(2026, 1, 21), "REV-V1", default));
        await db.SaveChangesAsync();
        open = await db.Set<VendTransOpen>().AsNoTracking().Where(x => x.RefRecId == 20).OrderBy(x => x.DueDate).ToListAsync();
        Assert.Equal(new[] { -400m, -300m, -300m }, open.Select(x => x.AmountCur));
        Assert.Equal(InstallmentDue, open.Select(x => x.DueDate));
        Assert.Equal(0m, (await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == 20)).SettleAmountCur);
    }
}
