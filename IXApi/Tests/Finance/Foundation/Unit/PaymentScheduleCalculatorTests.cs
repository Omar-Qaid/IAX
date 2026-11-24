using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.PaymentTerms;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>Installments by the D365 payment schedule allocation methods (PaymSchedBy).</summary>
public sealed class PaymentScheduleCalculatorTests
{
    private static readonly DateTime Due = new(2026, 5, 15);

    [Fact]
    public void NumOfPayment_splits_equally_spaces_by_the_interval_and_puts_rounding_on_the_last_installment()
    {
        var schedule = new PaymSched { Name = "S", PayBy = PaymSchedBy.NumOfPayment, NumOfPayment = 3, QtyUnit = 1, PeriodUnit = PeriodUnit.Month };
        var result = PaymentScheduleCalculator.Calculate(1000m, Due, schedule, []);
        Assert.Equal(new[] { 333.33m, 333.33m, 333.34m }, result.Select(x => x.Amount));
        Assert.Equal(new[] { Due, Due.AddMonths(1), Due.AddMonths(2) }, result.Select(x => x.DueDate));
    }

    [Fact]
    public void A_payable_keeps_its_negative_sign_on_every_installment()
    {
        var schedule = new PaymSched { Name = "S", PayBy = PaymSchedBy.NumOfPayment, NumOfPayment = 2, QtyUnit = 30, PeriodUnit = PeriodUnit.Day };
        var result = PaymentScheduleCalculator.Calculate(-2875m, Due, schedule, []);
        Assert.Equal(new[] { -1437.50m, -1437.50m }, result.Select(x => x.Amount));
        Assert.Equal(Due.AddDays(30), result[1].DueDate);
    }

    [Fact]
    public void AmountByPayment_pays_the_fixed_amount_until_the_remainder()
    {
        var schedule = new PaymSched { Name = "S", PayBy = PaymSchedBy.AmountByPayment, AmountCur = 400m, QtyUnit = 2, PeriodUnit = PeriodUnit.Week };
        var result = PaymentScheduleCalculator.Calculate(1000m, Due, schedule, []);
        Assert.Equal(new[] { 400m, 400m, 200m }, result.Select(x => x.Amount));
        Assert.Equal(new[] { Due, Due.AddDays(14), Due.AddDays(28) }, result.Select(x => x.DueDate));
    }

    [Fact]
    public void Specified_percent_lines_use_each_lines_periods_after_the_due_date()
    {
        var schedule = new PaymSched { Name = "S", PayBy = PaymSchedBy.Specified, PeriodUnit = PeriodUnit.Month };
        PaymSchedLine[] lines =
        [
            new() { Name = "S", LineNum = 1, PercentAmount = PercentAmount.Percent, Value = 40m, Qty = 0 },
            new() { Name = "S", LineNum = 2, PercentAmount = PercentAmount.Percent, Value = 60m, Qty = 6 }
        ];
        var result = PaymentScheduleCalculator.Calculate(4600m, Due, schedule, lines);
        Assert.Equal(new[] { 1840m, 2760m }, result.Select(x => x.Amount));
        Assert.Equal(new[] { Due, Due.AddMonths(6) }, result.Select(x => x.DueDate));
    }

    [Fact]
    public void Specified_amount_lines_must_equal_the_invoice_and_percent_lines_must_total_100()
    {
        var schedule = new PaymSched { Name = "S", PayBy = PaymSchedBy.Specified };
        Assert.Throws<InvalidOperationException>(() => PaymentScheduleCalculator.Calculate(1000m, Due, schedule,
            [new() { Name = "S", LineNum = 1, PercentAmount = PercentAmount.Amount, Value = 900m }]));
        Assert.Throws<InvalidOperationException>(() => PaymentScheduleCalculator.Calculate(1000m, Due, schedule,
            [new() { Name = "S", LineNum = 1, PercentAmount = PercentAmount.Percent, Value = 90m }]));
        Assert.Throws<InvalidOperationException>(() => PaymentScheduleCalculator.Calculate(1000m, Due, schedule,
            [new() { Name = "S", LineNum = 1, PercentAmount = PercentAmount.Percent, Value = 50m },
             new() { Name = "S", LineNum = 2, PercentAmount = PercentAmount.Amount, Value = 500m }]));
    }

    [Fact]
    public void Total_allocation_is_refused_by_name_rather_than_guessed()
    {
        var schedule = new PaymSched { Name = "S", PayBy = PaymSchedBy.Total, NumOfPayment = 2 };
        var error = Assert.Throws<InvalidOperationException>(() => PaymentScheduleCalculator.Calculate(1000m, Due, schedule, []));
        Assert.Contains("Total", error.Message);
    }

    [Fact]
    public void Open_rows_carry_each_installments_share_of_the_accounting_and_reporting_amounts()
    {
        var schedule = new PaymSched { Name = "S", PayBy = PaymSchedBy.NumOfPayment, NumOfPayment = 3, QtyUnit = 1, PeriodUnit = PeriodUnit.Month };
        var installments = PaymentScheduleCalculator.Calculate(100m, Due, schedule, []);
        var rows = PaymentScheduleOpenRows.Split(installments, 100m, 375m, 100m);
        Assert.Equal(new[] { 33.33m, 33.33m, 33.34m }, rows.Select(x => x.AmountCur));
        Assert.Equal(new[] { 124.99m, 124.99m, 125.02m }, rows.Select(x => x.AmountMst));
        Assert.Equal(375m, rows.Sum(x => x.AmountMst));
        Assert.Equal(100m, rows.Sum(x => x.ReportingAmount));
    }
}
