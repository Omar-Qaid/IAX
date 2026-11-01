using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount
{
    public class MainAccountService : BaseService<MainAccount>, IMainAccountService
    {
        protected override async Task OnBeforeAddAsync(MainAccount entity, CancellationToken ct)
        {
            var area = string.IsNullOrWhiteSpace(entity.DataAreaId) ? _currentUser.GetDataAreaId() : entity.DataAreaId;
            if (string.IsNullOrWhiteSpace(area)) throw new InvalidOperationException("A company must be selected.");
            entity.DataAreaId = area;
            await ValidateAsync(entity, ct);
            if (await _unitOfWork.Context.Set<MainAccount>().IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == area && x.LedgerChartOfAccounts == entity.LedgerChartOfAccounts
                    && x.MainAccountId == entity.MainAccountId && !x.IsDeleted, ct))
                throw new InvalidOperationException($"Main account '{entity.MainAccountId}' already exists in the chart of accounts.");
        }

        public override async Task<MainAccount> UpdateAsync(MainAccount entity, CancellationToken ct = default)
        {
            var current = await _unitOfWork.Context.Set<MainAccount>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.RecId == entity.RecId, ct)
                ?? throw new InvalidOperationException("Main account was not found.");
            entity.DataAreaId = current.DataAreaId;
            await ValidateAsync(entity, ct);
            if (await _unitOfWork.Context.Set<MainAccount>().IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == current.DataAreaId && x.LedgerChartOfAccounts == entity.LedgerChartOfAccounts
                    && x.MainAccountId == entity.MainAccountId && x.RecId != entity.RecId && !x.IsDeleted, ct))
                throw new InvalidOperationException($"Main account '{entity.MainAccountId}' already exists in the chart of accounts.");
            return await base.UpdateAsync(entity, ct);
        }

        private async Task ValidateAsync(MainAccount entity, CancellationToken ct)
        {
            if (!await _unitOfWork.Context.Set<LedgerChartOfAccounts>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.LedgerChartOfAccounts, ct))
                throw new InvalidOperationException("The chart of accounts does not exist in the selected company.");
            if (!string.IsNullOrWhiteSpace(entity.CurrencyCode) && !await _unitOfWork.Context.Set<Currency>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.CurrencyCode == entity.CurrencyCode, ct))
                throw new InvalidOperationException("The main-account currency does not exist in the selected company.");
        }

        public MainAccountService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public MainAccountService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
