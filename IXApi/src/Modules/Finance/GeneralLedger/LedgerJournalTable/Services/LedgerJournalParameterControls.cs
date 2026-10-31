using ParametersEntity = IAX.IXApi.Modules.Finance.Entities.LedgerParameters;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

/// <summary>
/// General ledger parameters that govern journal posting but that this journal path does not
/// implement. They were being read by nobody, so a company could switch a posting control on and
/// post as though it were in force.
///
/// Two kinds, handled differently on purpose:
///
/// A control switched on is a restriction. Posting without it is looser than the company
/// configured, so <see cref="Refusal"/> refuses the journal and names the parameter, rather than
/// posting accounting the configuration was meant to prevent. These parameters are off in the
/// default record, so only a company that deliberately switched one on is affected.
///
/// The rest cannot be refused, because their unimplemented state is stricter than configured
/// (a rounding tolerance that is never spent) or because refusing would block the default record
/// (AllowMultipleTransactionsWithinOneVoucher is No by default, and applying it needs D365's rule
/// for what counts as one transaction inside a voucher, which is not established here).
/// <see cref="Unenforced"/> reports those so the diagnostic does not imply they are applied.
///
/// The rounding tolerance also has nowhere to post a residual: this module has no automatic
/// transaction account setup, and the core cycle (section 9.12) warns against silent plugs to a
/// suspense account. Until that setup exists the exact-balance requirement in
/// <see cref="LedgerJournalPosting"/> stands, which is stricter and never wrong.
/// </summary>
public static class LedgerJournalParameterControls
{
    /// <summary>
    /// The error refusing this journal, or null when no switched-on restriction applies. A company
    /// with no parameter record is unrestricted, as it is for ledger settlement.
    /// </summary>
    public static string? Refusal(ParametersEntity? parameters)
    {
        if (parameters == null) return null;
        if (parameters.CheckVoucher == NoYes.Yes)
            return Message("Check voucher");
        if (parameters.CheckContinuous == NoYes.Yes)
            return Message("Continuous voucher numbers");
        if (parameters.CheckJournalizing == NoYes.Yes)
            return Message("Check journalizing");
        return null;

        static string Message(string control) =>
            $"The general ledger parameter '{control}' is switched on for this company, and this journal path does not " +
            "apply it. Posting would be looser than the configured control. Turn the parameter off to post without it, " +
            "or implement the control before posting.";
    }

    /// <summary>
    /// Posting controls that are configured but not applied, for the setup diagnostic. Each entry
    /// says what is not applied, never that the journal is valid.
    /// </summary>
    public static IReadOnlyList<string> Unenforced(ParametersEntity? parameters)
    {
        var items = new List<string>();
        if (parameters == null) return items;
        if (parameters.AllowMultipleTransactionsWithinOneVoucher == NoYes.No)
            items.Add("'Allow multiple transactions within one voucher' is No, and this journal path does not restrict a voucher to one transaction.");
        if (parameters.MaxRoundingDifferenceMst != 0m || parameters.MaxRoundingDifferenceMstSecond != 0m)
            items.Add("A rounding difference tolerance is configured, and is not applied: a voucher must still balance exactly, because no automatic transaction account is set up to carry a residual.");
        if (parameters.AbortErrorAccount != 0)
            items.Add("An error account is configured, and is not used: posting refuses a journal it cannot post instead of diverting an amount to that account.");
        if (parameters.LimitedOpenFiscalPeriods == NoYes.Yes)
            items.Add("'Limited open fiscal periods' is switched on, and is not applied: period access is checked from the ledger period status alone.");
        return items;
    }
}
