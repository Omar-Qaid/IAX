using System.Linq.Expressions;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar
{
    /// <summary>
    /// Ledger period status is posting control data: <see cref="Common.SubledgerPostingSupport"/>
    /// refuses to post into a period that is not Open, so an unguarded write here would reopen a
    /// closed period and let accounting into a signed-off month.
    ///
    /// Reads stay generic. The status changes only through <see cref="SetStatusAsync"/>, which
    /// applies the transition rules in <see cref="FiscalPeriodStatusTransition"/>. Generic update
    /// and delete are refused, and a create may not duplicate an existing ledger period, because
    /// a second row makes the period's status — and therefore posting — ambiguous.
    /// </summary>
    public class LedgerFiscalCalendarPeriodService : BaseService<LedgerFiscalCalendarPeriod>, ILedgerFiscalCalendarPeriodService
    {
        private DbContext Db => _unitOfWork.Context;

        public LedgerFiscalCalendarPeriodService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public LedgerFiscalCalendarPeriodService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }

        public async Task<(LedgerFiscalCalendarPeriodStatusResultDto? Result, string? Error)> SetStatusAsync(
            long recId, FiscalPeriodStatus status, CancellationToken ct = default)
        {
            var row = await Db.Set<LedgerFiscalCalendarPeriod>().SingleOrDefaultAsync(x => x.RecId == recId, ct);
            if (row == null) return (null, null); // Not found in the selected company; the caller reports 404.

            var refusal = FiscalPeriodStatusTransition.Check(row.Status, status);
            if (refusal != null) return (null, refusal);

            var period = await Db.Set<FiscalCalendarPeriod>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.RecId == row.FiscalCalendarPeriod, ct);
            if (period == null)
                return (null, "The ledger period does not reference a fiscal calendar period.");

            var previous = row.Status;
            row.Status = status;
            await _unitOfWork.CompleteAsync(ct);
            return (new LedgerFiscalCalendarPeriodStatusResultDto(row.RecId, row.Ledger, row.FiscalCalendarPeriod,
                period.Name, period.StartDate, period.EndDate, previous, status), null);
        }

        /// <summary>One status row per ledger and period, so posting resolves it unambiguously.</summary>
        protected override async Task OnBeforeAddAsync(LedgerFiscalCalendarPeriod entity, CancellationToken cancellationToken)
        {
            if (await Db.Set<LedgerFiscalCalendarPeriod>().AnyAsync(
                    x => x.Ledger == entity.Ledger && x.FiscalCalendarPeriod == entity.FiscalCalendarPeriod, cancellationToken))
                throw new InvalidOperationException(
                    "This ledger period already has a status row. Change its status instead of adding another.");
            if (entity.Status is FiscalPeriodStatus.Closed or FiscalPeriodStatus.YearClosed)
                throw new InvalidOperationException(
                    $"A ledger period cannot be created as {entity.Status}; create it Open or On hold and close it afterwards.");
            await base.OnBeforeAddAsync(entity, cancellationToken);
        }

        private static NotSupportedException Guarded() =>
            new("Ledger period status cannot be changed through generic services. Use the period status action, which refuses to reopen a permanently closed period.");

        public sealed override Task<LedgerFiscalCalendarPeriod> UpdateAsync(
            LedgerFiscalCalendarPeriod entity, CancellationToken cancellationToken = default) => throw Guarded();

        public sealed override Task<IEnumerable<LedgerFiscalCalendarPeriod>> UpdateRangeAsync(
            IEnumerable<LedgerFiscalCalendarPeriod> entities, CancellationToken cancellationToken = default) => throw Guarded();

        public sealed override Task<LedgerFiscalCalendarPeriod> UpdateWithDetailsAsync<TDetail>(
            LedgerFiscalCalendarPeriod entity, Expression<Func<LedgerFiscalCalendarPeriod, IEnumerable<TDetail>>> detailSelector,
            CancellationToken cancellationToken = default) => throw Guarded();

        public sealed override Task RemoveAsync(
            LedgerFiscalCalendarPeriod entity, CancellationToken cancellationToken = default) => throw Guarded();

        public sealed override Task RemoveRangeAsync(
            IEnumerable<LedgerFiscalCalendarPeriod> entities, CancellationToken cancellationToken = default) => throw Guarded();
    }
}
