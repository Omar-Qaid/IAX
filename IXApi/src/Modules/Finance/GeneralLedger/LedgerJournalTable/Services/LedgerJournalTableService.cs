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

        /// <summary>
        /// The static checks, combined: the setup check and the voucher arithmetic. Reads only, and
        /// never claims posting will succeed — Simulate is the authoritative answer.
        /// </summary>
        public async Task<LedgerJournalValidateResultDto?> ValidateAsync(long recId, CancellationToken ct)
        {
            var area = _currentUser.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return null;
            var db = _unitOfWork.Context;
            var header = await db.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            if (header == null) return null;

            var setup = await new LedgerJournalSetupCheck(db).CheckAsync(area, recId, ct);
            var lines = await db.Set<LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.JournalNum == header.JournalNum).ToListAsync(ct);
            var errors = new List<string>(setup?.Errors ?? []);
            errors.AddRange(LedgerVoucherBalanceCheck.Check(lines));
            return new(header.JournalNum, errors.Distinct().ToList(), setup?.UnverifiedChecks ?? []);
        }

        /// <summary>
        /// Runs the real posting path and rolls it back, so the answer comes from the rules that
        /// actually post rather than from a second implementation that could drift from them.
        /// </summary>
        public Task<(LedgerJournalSimulateResultDto? Result, string? Error)> SimulateAsync(long recId, CancellationToken ct) =>
            _unitOfWork.Context.Database.CreateExecutionStrategy().ExecuteAsync(() => SimulateCoreAsync(recId, ct));

        private async Task<(LedgerJournalSimulateResultDto? Result, string? Error)> SimulateCoreAsync(long recId, CancellationToken ct)
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
                    db.ChangeTracker.Clear();
                    return (null, null);
                }

                LedgerJournalSimulateResultDto result;
                try
                {
                    var posted = await new LedgerJournalPosting(db).PostAsync(area, header, ct);
                    await _unitOfWork.CompleteAsync(ct); // Inside the transaction, so the writes are exercised.
                    result = Describe(header.JournalNum, posted, await SimulatedEntriesAsync(db, area, header.JournalNum, ct));
                }
                catch (SubledgerPostingException ex)
                {
                    // A refusal is the useful answer, not a failure of the simulation.
                    result = new(header.JournalNum, WouldPost: false, Refusal: ex.Message, 0, 0, 0m, 0m, []);
                }
                return (result, null);
            }
            finally
            {
                // Always rolled back: a simulation never leaves posted accounting, a posted journal
                // status or a consumed voucher behind. Clearing the tracker drops the in-memory
                // mutations the posting path made, so nothing leaks into a later save.
                await _unitOfWork.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
            }
        }

        private static LedgerJournalSimulateResultDto Describe(string journalNum,
            LedgerJournalPostResultDto posted, IReadOnlyList<LedgerJournalSimulatedEntryDto> entries) =>
            new(journalNum, WouldPost: true, Refusal: null, posted.VouchersPosted, posted.AccountEntriesPosted,
                posted.TotalDebit, posted.TotalCredit, entries);

        /// <summary>The account entries the simulated posting created, read back before the rollback.</summary>
        private static async Task<IReadOnlyList<LedgerJournalSimulatedEntryDto>> SimulatedEntriesAsync(
            DbContext db, string area, string journalNum, CancellationToken ct) =>
            await (from entry in db.Set<GeneralJournalEntry>().AsNoTracking()
                   join line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                       on entry.RecId equals line.GeneralJournalEntry
                   where entry.DataAreaId == area && entry.JournalNumber == journalNum
                   orderby entry.AccountingDate, entry.SubledgerVoucher, line.RecId
                   select new LedgerJournalSimulatedEntryDto(entry.SubledgerVoucher, entry.AccountingDate,
                       line.LedgerAccount, line.PostingType.ToString(), line.TransactionCurrencyCode,
                       line.AccountingCurrencyAmount, line.Text)).ToListAsync(ct);

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
