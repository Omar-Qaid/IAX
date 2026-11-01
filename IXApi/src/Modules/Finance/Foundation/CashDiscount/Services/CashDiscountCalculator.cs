using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.CashDiscount;

/// <summary>
/// The cash discount date and amount, alongside <c>PaymentDueDateCalculator</c> and following its
/// shape: a static calculation that refuses what it cannot do rather than approximating it.
/// </summary>
public static class CashDiscountCalculator
{
    /// <summary>
    /// The last date the discount may be taken: the base date plus the discount's offset.
    /// </summary>
    public static DateTime Date(DateTime baseDate, CashDisc discount)
    {
        if (discount.NumOfMonths < 0 || discount.NumOfDays < 0)
            throw new InvalidOperationException("Cash discount offsets cannot be negative.");
        return baseDate.Date.AddMonths(discount.NumOfMonths).AddDays(discount.NumOfDays);
    }

    /// <summary>
    /// The discount on an amount, rounded to two places. The amount carries the sign of the
    /// transaction it is taken from, so a receivable discount is positive and a payable one is
    /// negative, and the caller does not have to re-derive the side.
    /// </summary>
    public static decimal Amount(decimal amount, CashDisc discount)
    {
        if (discount.DiscPct < 0m || discount.DiscPct > 100m)
            throw new InvalidOperationException("The cash discount percentage must be between 0 and 100.");
        return decimal.Round(amount * discount.DiscPct / 100m, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Whether a payment on <paramref name="paymentDate"/> is in time for the discount.
    /// </summary>
    public static bool IsWithinPeriod(DateTime paymentDate, DateTime discountDate) =>
        paymentDate.Date <= discountDate.Date;
}
