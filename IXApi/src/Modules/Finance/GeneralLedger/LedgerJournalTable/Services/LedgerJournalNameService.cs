using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    public class LedgerJournalNameService : BaseService<IAX.IXApi.Modules.Finance.Entities.LedgerJournalName>, ILedgerJournalNameService
    {
        protected override async Task OnBeforeAddAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalName entity, CancellationToken ct)
        {
            var area = string.IsNullOrWhiteSpace(entity.DataAreaId) ? _currentUser.GetDataAreaId() : entity.DataAreaId;
            if (string.IsNullOrWhiteSpace(area)) throw new InvalidOperationException("A company must be selected.");
            entity.DataAreaId = area;
            await ValidateSetupAsync(entity, ct);
        }

        public override async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalName> UpdateAsync(
            IAX.IXApi.Modules.Finance.Entities.LedgerJournalName entity, CancellationToken ct = default)
        {
            var current = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalName>()
                .AsNoTracking().SingleOrDefaultAsync(x => x.RecId == entity.RecId, ct)
                ?? throw new InvalidOperationException("Journal name was not found.");
            entity.DataAreaId = current.DataAreaId;
            if (!string.Equals(entity.JournalName, current.JournalName, StringComparison.Ordinal))
                throw new InvalidOperationException("A journal name identifier cannot be changed after it is created.");
            await ValidateSetupAsync(entity, ct);
            return await base.UpdateAsync(entity, ct);
        }

        private async Task ValidateSetupAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalName entity, CancellationToken ct)
        {
            var db = _unitOfWork.Context;
            if (!await db.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.NumberSequenceTable && !x.IsDeleted, ct))
                throw new InvalidOperationException("The voucher number sequence does not exist in the selected company.");
            if (!await db.Set<Currency>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.CurrencyCode == entity.CurrencyCode, ct))
                throw new InvalidOperationException("The journal currency does not exist in the selected company.");
            if (entity.DefaultDimension > 0 && !await db.Set<DimensionAttributeValueSet>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.DefaultDimension, ct))
                throw new InvalidOperationException("The journal default dimension set does not exist in the selected company.");
            if (entity.OffsetLedgerDimension > 0 && !await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == entity.DataAreaId && x.RecId == entity.OffsetLedgerDimension, ct))
                throw new InvalidOperationException("The journal offset ledger dimension does not exist in the selected company.");
        }

        public LedgerJournalNameService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public LedgerJournalNameService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
