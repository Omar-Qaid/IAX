using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Xunit;

namespace IAX.IXApi.Tests;

public class LedgerVoucherBalanceTests
{
    private static LedgerJournalTrans Line(decimal debit, decimal credit, string voucher = "V1", string currency = "SAR") =>
        new() { DataAreaId = "dat", Company = "dat", Voucher = voucher, CurrencyCode = currency,
            TransDate = new DateTime(2026, 1, 1), AmountCurDebit = debit, AmountCurCredit = credit };

    [Fact]
    public void Balanced_batch_cannot_hide_unbalanced_individual_vouchers() =>
        Assert.Equal(2, LedgerVoucherBalanceCheck.Check([Line(100, 0), Line(0, 100, "V2")]).Count);

    [Fact]
    public void Different_currencies_and_dates_do_not_cancel()
    {
        Assert.Equal(2, LedgerVoucherBalanceCheck.Check([Line(100, 0), Line(0, 100, currency: "USD")]).Count);
        var credit = Line(0, 100);
        credit.TransDate = credit.TransDate.AddDays(1);
        Assert.Equal(2, LedgerVoucherBalanceCheck.Check([Line(100, 0), credit]).Count);
    }

    [Fact]
    public void Explicit_lines_and_offset_representation_balance()
    {
        Assert.Empty(LedgerVoucherBalanceCheck.Check([Line(100, 0), Line(0, 100)]));
        Assert.Empty(LedgerVoucherBalanceCheck.Check([Line(-100, 0), Line(0, -100)]));
        var offset = Line(100, 0);
        offset.OffsetLedgerDimension = 1;
        Assert.Empty(LedgerVoucherBalanceCheck.Check([offset]));
        Assert.NotEmpty(LedgerVoucherBalanceCheck.Check([offset, Line(0, 100)]));
    }

    [Fact]
    public void Empty_and_two_sided_lines_are_rejected()
    {
        Assert.NotEmpty(LedgerVoucherBalanceCheck.Check([]));
        Assert.NotEmpty(LedgerVoucherBalanceCheck.Check([Line(100, 100)]));
    }
}
