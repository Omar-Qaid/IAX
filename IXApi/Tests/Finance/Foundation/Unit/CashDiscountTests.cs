using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.CashDiscount;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The cash discount date and amount.
///
/// The period is modelled on PaymTerm's own NumOfMonths/NumOfDays pair, because the cycle docs
/// name CashDisc only as a field and never specify the table. The calculator refuses what it
/// cannot do rather than approximating it, as PaymentDueDateCalculator does.
/// </summary>
public class CashDiscountTests
{
    [Theory]
    [InlineData(0, 10, "2026-10-11")]
    [InlineData(0, 0, "2026-10-01")]
    [InlineData(1, 0, "2026-11-01")]
    [InlineData(1, 15, "2026-11-16")]
    public void The_discount_date_is_the_base_date_plus_the_offset(int months, int days, string expected)
    {
        var discount = new CashDisc { NumOfMonths = months, NumOfDays = days };

        var date = CashDiscountCalculator.Date(new DateTime(2026, 10, 1), discount);

        Assert.Equal(DateTime.Parse(expected), date);
    }

    [Fact]
    public void A_negative_offset_is_refused()
    {
        var discount = new CashDisc { NumOfDays = -1 };

        var error = Assert.Throws<InvalidOperationException>(
            () => CashDiscountCalculator.Date(new DateTime(2026, 10, 1), discount));

        Assert.Contains("cannot be negative", error.Message);
    }

    [Theory]
    [InlineData(1000, 2, 20)]
    [InlineData(1000, 2.5, 25)]
    [InlineData(100, 0, 0)]
    // Rounds away from zero at two places: 2% of 333.33 is 6.6666.
    [InlineData(333.33, 2, 6.67)]
    public void The_discount_amount_is_the_percentage_rounded_to_two_places(
        decimal amount, decimal percent, decimal expected)
    {
        var discount = new CashDisc { DiscPct = percent };

        Assert.Equal(expected, CashDiscountCalculator.Amount(amount, discount));
    }

    /// <summary>
    /// The amount keeps the sign of what it is taken from, so a payable discount comes back
    /// negative and the caller does not have to re-derive the side.
    /// </summary>
    [Fact]
    public void A_discount_on_a_credit_is_negative()
    {
        var discount = new CashDisc { DiscPct = 2m };

        Assert.Equal(-20m, CashDiscountCalculator.Amount(-1000m, discount));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(-1)]
    public void A_percentage_outside_zero_to_one_hundred_is_refused(decimal percent)
    {
        var discount = new CashDisc { DiscPct = percent };

        var error = Assert.Throws<InvalidOperationException>(() => CashDiscountCalculator.Amount(1000m, discount));

        Assert.Contains("between 0 and 100", error.Message);
    }

    [Theory]
    // On the discount date is still in time; the day after is not.
    [InlineData("2026-10-10", true)]
    [InlineData("2026-10-11", true)]
    [InlineData("2026-10-12", false)]
    public void The_period_includes_the_discount_date_itself(string paymentDate, bool expected)
    {
        var within = CashDiscountCalculator.IsWithinPeriod(
            DateTime.Parse(paymentDate), new DateTime(2026, 10, 11));

        Assert.Equal(expected, within);
    }
}
