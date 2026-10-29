using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    public class LedgerJournalTableService : BaseService<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>, ILedgerJournalTableService
    {
        public Task<LedgerJournalSetupCheckDto?> CheckSetupAsync(long recId, CancellationToken ct) =>
            new LedgerJournalSetupCheck(_unitOfWork.Context).CheckAsync(_currentUser.GetDataAreaId(), recId, ct);

        public async Task<IReadOnlyList<string>?> CheckVoucherBalancesAsync(long recId, CancellationToken ct)
        {
            var area = _currentUser.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return null;
            var header = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>()
                .AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            if (header == null) return null;
            var lines = await _unitOfWork.Context.Set<LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.JournalNum == header.JournalNum).ToListAsync(ct);
            return LedgerVoucherBalanceCheck.Check(lines);
        }

        public Task<(LedgerJournalPostResultDto? Result, string? Error)> PostAsync(long recId, CancellationToken ct) =>
            _unitOfWork.Context.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(recId, ct));

        private async Task<(LedgerJournalPostResultDto? Result, string? Error)> PostCoreAsync(long recId, CancellationToken ct)
        {
            var area = _currentUser.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
            var db = _unitOfWork.Context;
            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var header = await db.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
                if (header == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return (null, null);
                }
                var result = await new LedgerJournalPosting(db).PostAsync(area, header, ct);
                await _unitOfWork.CompleteAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return (result, null);
            }
            catch (SubledgerPostingException ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                return (null, ex.Message);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                throw;
            }
        }

        public LedgerJournalTableService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public LedgerJournalTableService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
