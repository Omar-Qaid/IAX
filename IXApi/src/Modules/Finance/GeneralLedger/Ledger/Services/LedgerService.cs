using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Ledger
{
    public class LedgerService : BaseService<IAX.IXApi.Modules.Finance.Entities.Ledger>, ILedgerService
    {
        protected override async Task OnBeforeAddAsync(IAX.IXApi.Modules.Finance.Entities.Ledger entity, CancellationToken cancellationToken)
        {
            var area = string.IsNullOrWhiteSpace(entity.DataAreaId) ? _currentUser.GetDataAreaId() : entity.DataAreaId;
            if (string.IsNullOrWhiteSpace(area)) throw new InvalidOperationException("A company must be selected.");
            if (await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.Ledger>()
                .IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == area && !x.IsDeleted, cancellationToken))
                throw new InvalidOperationException("Only one ledger can be configured for a legal entity.");
            entity.DataAreaId = area;
        }

        public override async Task<IAX.IXApi.Modules.Finance.Entities.Ledger> UpdateAsync(
            IAX.IXApi.Modules.Finance.Entities.Ledger entity, CancellationToken cancellationToken = default)
        {
            var current = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.Ledger>()
                .AsNoTracking().SingleOrDefaultAsync(x => x.RecId == entity.RecId, cancellationToken)
                ?? throw new InvalidOperationException("Ledger was not found.");
            var hasPostings = await _unitOfWork.Context.Set<GeneralJournalEntry>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == current.DataAreaId, cancellationToken);
            if (hasPostings && (current.ChartOfAccounts != entity.ChartOfAccounts
                || current.FiscalCalendar != entity.FiscalCalendar
                || current.AccountingCurrency != entity.AccountingCurrency
                || current.ReportingCurrency != entity.ReportingCurrency
                || current.DefaultExchangeRateType != entity.DefaultExchangeRateType
                || current.ReportingCurrencyExchangeRateType != entity.ReportingCurrencyExchangeRateType))
                throw new InvalidOperationException("Chart of accounts, fiscal calendar, currencies, and exchange-rate types cannot be changed after ledger postings exist.");
            return await base.UpdateAsync(entity, cancellationToken);
        }

        public override async Task RemoveAsync(IAX.IXApi.Modules.Finance.Entities.Ledger entity, CancellationToken cancellationToken = default)
        {
            if (await _unitOfWork.Context.Set<GeneralJournalEntry>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == entity.DataAreaId, cancellationToken))
                throw new InvalidOperationException("A ledger with posted entries cannot be deleted.");
            await base.RemoveAsync(entity, cancellationToken);
        }

        public LedgerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public LedgerService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
