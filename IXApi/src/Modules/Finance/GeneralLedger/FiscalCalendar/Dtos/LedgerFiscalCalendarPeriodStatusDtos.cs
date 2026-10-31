using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;

/// <summary>The status to set on a ledger period.</summary>
public sealed record LedgerFiscalCalendarPeriodStatusDto(FiscalPeriodStatus Status);

/// <summary>The period's status after the change, and the status it held before.</summary>
public sealed record LedgerFiscalCalendarPeriodStatusResultDto(
    long RecId,
    long Ledger,
    long FiscalCalendarPeriod,
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate,
    FiscalPeriodStatus PreviousStatus,
    FiscalPeriodStatus Status);
