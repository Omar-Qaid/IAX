using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;

/// <summary>
/// The ledger period status machine from the core cycle (D365_FO_Core_General_Ledger_Full_Cycle.md
/// §11): "Use On hold for a normal reversible posting restriction. Permanently closed cannot be
/// reopened; do not use it as the ordinary month end switch."
///
/// Open and On hold convert into each other, and either may be permanently closed. Closed is
/// terminal. Year closed is produced by the year end close process, which is not implemented, so
/// it is neither set nor left from here.
/// </summary>
public static class FiscalPeriodStatusTransition
{
    /// <summary>
    /// The error that refuses this change, or null when it is allowed. A change to the status the
    /// period already has is allowed and does nothing.
    /// </summary>
    public static string? Check(FiscalPeriodStatus from, FiscalPeriodStatus to)
    {
        if (!Enum.IsDefined(to)) return $"'{(int)to}' is not a ledger period status.";
        if (from == to) return null;
        return (from, to) switch
        {
            (FiscalPeriodStatus.Closed, _) =>
                "A permanently closed period cannot be reopened. Post the entry to an open period instead.",
            (FiscalPeriodStatus.YearClosed, _) =>
                "A year closed period is maintained by the year end close process and cannot be changed here.",
            (_, FiscalPeriodStatus.YearClosed) =>
                "Year closed is set by the year end close process, not by a period status change.",
            (FiscalPeriodStatus.Open, FiscalPeriodStatus.OnHold) => null,
            (FiscalPeriodStatus.OnHold, FiscalPeriodStatus.Open) => null,
            (FiscalPeriodStatus.Open, FiscalPeriodStatus.Closed) => null,
            (FiscalPeriodStatus.OnHold, FiscalPeriodStatus.Closed) => null,
            _ => $"A ledger period cannot change from {from} to {to}."
        };
    }
}
