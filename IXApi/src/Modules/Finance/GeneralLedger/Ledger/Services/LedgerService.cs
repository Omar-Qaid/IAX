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
            entity.PrimaryForLegalEntity = await _unitOfWork.Context.Set<CompanyInfo>().AsNoTracking()
                .Where(x => x.DataAreaId == area).Select(x => x.RecId).SingleOrDefaultAsync(cancellationToken);
            if (entity.PrimaryForLegalEntity == 0)
                throw new InvalidOperationException("The selected company has no legal-entity record.");
            await ValidateSetupAsync(entity, cancellationToken);
        }

        public override async Task<IAX.IXApi.Modules.Finance.Entities.Ledger> UpdateAsync(
            IAX.IXApi.Modules.Finance.Entities.Ledger entity, CancellationToken cancellationToken = default)
        {
            var current = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.Ledger>()
                .AsNoTracking().SingleOrDefaultAsync(x => x.RecId == entity.RecId, cancellationToken)
                ?? throw new InvalidOperationException("Ledger was not found.");
            entity.DataAreaId = current.DataAreaId;
            entity.PrimaryForLegalEntity = current.PrimaryForLegalEntity;
            await ValidateSetupAsync(entity, cancellationToken);
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

        private async Task ValidateSetupAsync(IAX.IXApi.Modules.Finance.Entities.Ledger entity, CancellationToken ct)
        {
            var db = _unitOfWork.Context;
            if (!await db.Set<LedgerChartOfAccounts>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.ChartOfAccounts, ct))
                throw new InvalidOperationException("The chart of accounts does not exist in the selected company.");
            if (!await db.Set<IAX.IXApi.Modules.Finance.Entities.FiscalCalendar>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.FiscalCalendar, ct))
                throw new InvalidOperationException("The fiscal calendar does not exist in the selected company.");
            if (!await db.Set<Currency>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.CurrencyCode == entity.AccountingCurrency, ct))
                throw new InvalidOperationException("The accounting currency does not exist in the selected company.");
            if (!string.IsNullOrWhiteSpace(entity.ReportingCurrency) && !await db.Set<Currency>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.CurrencyCode == entity.ReportingCurrency, ct))
                throw new InvalidOperationException("The reporting currency does not exist in the selected company.");
            if (!await db.Set<ExchangeRateType>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.DefaultExchangeRateType, ct))
                throw new InvalidOperationException("The default exchange-rate type does not exist in the selected company.");
            if (!string.IsNullOrWhiteSpace(entity.ReportingCurrency) && !await db.Set<ExchangeRateType>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.ReportingCurrencyExchangeRateType, ct))
                throw new InvalidOperationException("The reporting exchange-rate type does not exist in the selected company.");
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
