using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Xunit;
using ParametersEntity = IAX.IXApi.Modules.Finance.Entities.LedgerParameters;

namespace IAX.IXApi.Tests;

/// <summary>
/// These parameters were read by nobody, so a company could switch a posting control on and post
/// as though it were applied. A switched-on restriction now refuses the journal; a control whose
/// unimplemented state is stricter than configured is reported instead.
/// </summary>
public class LedgerJournalParameterControlsTests
{
    // A company that never opened the parameters page has no record and is unrestricted.
    [Fact]
    public void No_parameter_record_restricts_nothing()
    {
        Assert.Null(LedgerJournalParameterControls.Refusal(null));
        Assert.Empty(LedgerJournalParameterControls.Unenforced(null));
    }

    // The default record switches nothing on, so posting is unaffected by its existence.
    [Fact]
    public void The_default_record_refuses_nothing_and_reports_only_the_voucher_restriction()
    {
        var parameters = new ParametersEntity { DataAreaId = "dat" };
        Assert.Null(LedgerJournalParameterControls.Refusal(parameters));
        // No is the cleared default, and the path does not restrict a voucher to one transaction.
        Assert.Contains(LedgerJournalParameterControls.Unenforced(parameters),
            x => x.Contains("one voucher"));
    }

    [Theory]
    [InlineData(nameof(ParametersEntity.CheckVoucher), "Check voucher")]
    [InlineData(nameof(ParametersEntity.CheckContinuous), "Continuous voucher numbers")]
    [InlineData(nameof(ParametersEntity.CheckJournalizing), "Check journalizing")]
    public void A_switched_on_restriction_refuses_the_journal_and_names_the_parameter(string property, string label)
    {
        var parameters = new ParametersEntity { DataAreaId = "dat" };
        typeof(ParametersEntity).GetProperty(property)!.SetValue(parameters, NoYes.Yes);

        var refusal = LedgerJournalParameterControls.Refusal(parameters);
        Assert.NotNull(refusal);
        Assert.Contains(label, refusal);
        Assert.Contains("does not apply it", refusal);
    }

    // A tolerance that is never spent is stricter than configured, so it must not block posting.
    [Fact]
    public void A_rounding_tolerance_is_reported_but_never_refuses_posting()
    {
        var parameters = new ParametersEntity { DataAreaId = "dat", MaxRoundingDifferenceMst = 0.05m };
        Assert.Null(LedgerJournalParameterControls.Refusal(parameters));
        Assert.Contains(LedgerJournalParameterControls.Unenforced(parameters),
            x => x.Contains("rounding difference tolerance") && x.Contains("balance exactly"));

        var second = new ParametersEntity { DataAreaId = "dat", MaxRoundingDifferenceMstSecond = 0.01m };
        Assert.Contains(LedgerJournalParameterControls.Unenforced(second),
            x => x.Contains("rounding difference tolerance"));
    }

    [Fact]
    public void An_error_account_and_limited_open_periods_are_reported_as_not_applied()
    {
        var parameters = new ParametersEntity
        {
            DataAreaId = "dat",
            AbortErrorAccount = 42,
            LimitedOpenFiscalPeriods = NoYes.Yes,
            AllowMultipleTransactionsWithinOneVoucher = NoYes.Yes
        };

        Assert.Null(LedgerJournalParameterControls.Refusal(parameters));
        var reported = LedgerJournalParameterControls.Unenforced(parameters);
        Assert.Contains(reported, x => x.Contains("error account"));
        Assert.Contains(reported, x => x.Contains("Limited open fiscal periods"));
        // Switched on, the voucher rule permits what the path already permits, so nothing to report.
        Assert.DoesNotContain(reported, x => x.Contains("one voucher"));
    }

    [Fact]
    public void Refusal_reports_one_control_at_a_time_so_the_message_names_a_single_parameter()
    {
        var parameters = new ParametersEntity
        {
            DataAreaId = "dat", CheckVoucher = NoYes.Yes, CheckContinuous = NoYes.Yes, CheckJournalizing = NoYes.Yes
        };
        var refusal = LedgerJournalParameterControls.Refusal(parameters)!;
        Assert.Contains("Check voucher", refusal);
        Assert.DoesNotContain("Check journalizing", refusal);
    }
}
