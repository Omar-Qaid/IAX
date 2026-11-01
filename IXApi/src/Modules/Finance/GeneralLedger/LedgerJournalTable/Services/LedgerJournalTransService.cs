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
            entity.OffsetCompany = string.IsNullOrWhiteSpace(entity.OffsetCompany) ? area! : entity.OffsetCompany;
            entity.CurrencyCode = string.IsNullOrWhiteSpace(entity.CurrencyCode) ? header.CurrencyCode : entity.CurrencyCode;
            if (header.FixedOffsetAccount == NoYes.Yes)
            {
                entity.OffsetAccountType = header.OffsetAccountType;
                entity.OffsetLedgerDimension = header.OffsetLedgerDimension;
            }
            if (entity.DocumentDate == default) entity.DocumentDate = entity.TransDate;
            entity.NoEdit = NoYes.No;
            if (entity.LineNum == 0)
                entity.LineNum = (await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>()
                    .Where(x => x.DataAreaId == area && x.JournalNum == entity.JournalNum)
                    .MaxAsync(x => (decimal?)x.LineNum, ct) ?? 0) + 1;
        }

        public override async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> UpdateAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct = default)
        {
            var current = await EnsureEditableAsync(entity.RecId, ct);
            entity.DataAreaId = current.DataAreaId;
            entity.JournalNum = current.JournalNum;
            entity.Company = current.Company;
            entity.NoEdit = current.NoEdit;
            return await base.UpdateAsync(entity, ct);
        }

        public override async Task RemoveAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct = default)
        {
            await EnsureEditableAsync(entity.RecId, ct);
            await base.RemoveAsync(entity, ct);
        }

        private async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> EnsureEditableAsync(long recId, CancellationToken ct)
        {
            var row = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.RecId == recId, ct)
                ?? throw new InvalidOperationException("Journal line was not found.");
            var posted = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == row.DataAreaId && x.JournalNum == row.JournalNum && x.Posted == NoYes.Yes, ct);
            if (row.NoEdit == NoYes.Yes || posted) throw new InvalidOperationException("Posted journal lines cannot be changed or deleted.");
            return row;
        }

        public LedgerJournalTransService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public LedgerJournalTransService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
