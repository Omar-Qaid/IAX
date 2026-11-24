using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.PaymentTerms;

/// <summary>One installment of an invoice under a payment schedule.</summary>
public sealed record PaymentInstallment(int Number, DateTime DueDate, decimal Amount);

/// <summary>
/// Splits an invoice amount into installments by a D365 payment schedule (<c>PaymSched</c>/<c>PaymSchedLine</c>).
/// The first installment falls on the due date the terms of payment produce; later ones are spaced by the
/// schedule's interval (<c>QtyUnit</c> x <c>PeriodUnit</c>) or, for <c>Specified</c>, by each line's <c>Qty</c>.
/// Installments carry the sign of the invoice amount, and rounding differences go to the last installment, so the
/// installments always add up to the invoice exactly.
/// </summary>
public static class PaymentScheduleCalculator
{
    public static IReadOnlyList<PaymentInstallment> Calculate(decimal total, DateTime firstDueDate, PaymSched schedule,
        IReadOnlyCollection<PaymSchedLine> lines, int decimals = 2)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        if (total == 0m) return [new(1, firstDueDate.Date, 0m)];
        var sign = Math.Sign(total);
        var magnitude = Math.Abs(total);
        return schedule.PayBy switch
        {
            PaymSchedBy.NumOfPayment => Equal(magnitude, sign, firstDueDate, schedule, decimals),
            PaymSchedBy.AmountByPayment => ByAmount(magnitude, sign, firstDueDate, schedule, decimals),
            PaymSchedBy.Specified => Specified(magnitude, sign, firstDueDate, schedule, lines, decimals),
            _ => throw new InvalidOperationException(
                $"Payment schedule '{schedule.Name}' uses allocation {schedule.PayBy}, which is not supported for installments.")
        };
    }

    private static IReadOnlyList<PaymentInstallment> Equal(decimal magnitude, int sign, DateTime first, PaymSched schedule, int decimals)
    {
        if (schedule.NumOfPayment < 1)
            throw new InvalidOperationException($"Payment schedule '{schedule.Name}' needs at least one payment.");
        RequireInterval(schedule);
        var each = decimal.Round(magnitude / schedule.NumOfPayment, decimals, MidpointRounding.AwayFromZero);
        return Build(Enumerable.Repeat(each, schedule.NumOfPayment).ToList(), magnitude, sign,
            i => Offset(first, schedule.PeriodUnit, i * schedule.QtyUnit));
    }

    private static IReadOnlyList<PaymentInstallment> ByAmount(decimal magnitude, int sign, DateTime first, PaymSched schedule, int decimals)
    {
        if (schedule.AmountCur <= 0m)
            throw new InvalidOperationException($"Payment schedule '{schedule.Name}' needs a positive amount per payment.");
        RequireInterval(schedule);
        var each = decimal.Round(schedule.AmountCur, decimals, MidpointRounding.AwayFromZero);
        var count = (int)Math.Ceiling(magnitude / each);
        return Build(Enumerable.Repeat(each, count).ToList(), magnitude, sign, i => Offset(first, schedule.PeriodUnit, i * schedule.QtyUnit));
    }

    private static IReadOnlyList<PaymentInstallment> Specified(decimal magnitude, int sign, DateTime first, PaymSched schedule,
        IReadOnlyCollection<PaymSchedLine> lines, int decimals)
    {
        var ordered = lines.Where(x => x.Name == schedule.Name).OrderBy(x => x.LineNum).ToList();
        if (ordered.Count == 0)
            throw new InvalidOperationException($"Payment schedule '{schedule.Name}' is set to Specified but has no lines.");
        if (ordered.Any(x => x.Value <= 0m || x.Qty < 0))
            throw new InvalidOperationException($"Payment schedule '{schedule.Name}' has a line with no value or a negative period.");
        if (ordered.All(x => x.PercentAmount == PercentAmount.Percent))
        {
            if (ordered.Sum(x => x.Value) != 100m)
                throw new InvalidOperationException($"Payment schedule '{schedule.Name}' lines must total 100 percent.");
        }
        else if (ordered.All(x => x.PercentAmount == PercentAmount.Amount))
        {
            if (ordered.Sum(x => x.Value) != magnitude)
                throw new InvalidOperationException($"Payment schedule '{schedule.Name}' line amounts do not equal the invoice amount.");
        }
        else throw new InvalidOperationException($"Payment schedule '{schedule.Name}' mixes percentage and amount lines.");
        var amounts = ordered.Select(x => x.PercentAmount == PercentAmount.Percent
            ? decimal.Round(magnitude * x.Value / 100m, decimals, MidpointRounding.AwayFromZero)
            : x.Value).ToList();
        return Build(amounts, magnitude, sign, i => Offset(first, schedule.PeriodUnit, ordered[i].Qty));
    }

    /// <summary>Signs the installments and moves the rounding difference onto the last one.</summary>
    private static IReadOnlyList<PaymentInstallment> Build(List<decimal> amounts, decimal magnitude, int sign, Func<int, DateTime> dueDate)
    {
        amounts[^1] += magnitude - amounts.Sum();
        if (amounts[^1] <= 0m)
            throw new InvalidOperationException("The payment schedule leaves no amount for its last installment.");
        return amounts.Select((amount, i) => new PaymentInstallment(i + 1, dueDate(i), sign * amount)).ToList();
    }

    private static void RequireInterval(PaymSched schedule)
    {
        if (schedule.QtyUnit < 0)
            throw new InvalidOperationException($"Payment schedule '{schedule.Name}' has a negative payment interval.");
    }

    private static DateTime Offset(DateTime first, PeriodUnit unit, int periods) => unit switch
    {
        PeriodUnit.Day => first.Date.AddDays(periods),
        PeriodUnit.Week => first.Date.AddDays(7 * periods),
        PeriodUnit.Month => first.Date.AddMonths(periods),
        PeriodUnit.Year => first.Date.AddYears(periods),
        _ => throw new InvalidOperationException($"Period unit {unit} is not supported.")
    };
}
