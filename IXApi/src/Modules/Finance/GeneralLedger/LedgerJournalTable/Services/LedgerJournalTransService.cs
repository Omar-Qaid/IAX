using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    public class LedgerJournalTransService : BaseService<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>, ILedgerJournalTransService
    {
        protected override async Task OnBeforeAddAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct)
        {
            var area = string.IsNullOrWhiteSpace(entity.DataAreaId) ? _currentUser.GetDataAreaId() : entity.DataAreaId;
            var header = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.JournalNum == entity.JournalNum, ct)
                ?? throw new InvalidOperationException("Journal header was not found.");
            if (header.Posted == NoYes.Yes) throw new InvalidOperationException("Lines cannot be added to a posted journal.");
            entity.DataAreaId = area!;
            entity.Company = string.IsNullOrWhiteSpace(entity.Company) ? area! : entity.Company;
            if (entity.LineNum == 0)
                entity.LineNum = (await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>()
                    .Where(x => x.DataAreaId == area && x.JournalNum == entity.JournalNum)
                    .MaxAsync(x => (decimal?)x.LineNum, ct) ?? 0) + 1;
        }

        public override async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> UpdateAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct = default)
        {
            await EnsureEditableAsync(entity.RecId, ct);
            return await base.UpdateAsync(entity, ct);
        }

        public override async Task RemoveAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct = default)
        {
            await EnsureEditableAsync(entity.RecId, ct);
            await base.RemoveAsync(entity, ct);
        }

        private async Task EnsureEditableAsync(long recId, CancellationToken ct)
        {
            var row = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.RecId == recId).Select(x => new { x.NoEdit, x.JournalNum, x.DataAreaId }).SingleOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("Journal line was not found.");
            var posted = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == row.DataAreaId && x.JournalNum == row.JournalNum && x.Posted == NoYes.Yes, ct);
            if (row.NoEdit == NoYes.Yes || posted) throw new InvalidOperationException("Posted journal lines cannot be changed or deleted.");
        }

        public LedgerJournalTransService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public LedgerJournalTransService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
