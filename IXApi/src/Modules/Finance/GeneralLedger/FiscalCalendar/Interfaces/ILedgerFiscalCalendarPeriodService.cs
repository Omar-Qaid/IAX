using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    public interface ILedgerFiscalCalendarPeriodService : IBaseService<LedgerFiscalCalendarPeriod>
    {
        /// <summary>
        /// Changes a ledger period's status under the core cycle's rules: Open and On hold convert
        /// into each other, either may be permanently closed, and a closed period never reopens.
        /// Returns a null result and a null error when the period is not in the selected company.
        /// </summary>
        Task<(LedgerFiscalCalendarPeriodStatusResultDto? Result, string? Error)> SetStatusAsync(
            long recId, FiscalPeriodStatus status, CancellationToken ct = default);
    }
}
