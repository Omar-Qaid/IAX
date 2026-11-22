using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    public class LedgerJournalTableService : BaseService<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>, ILedgerJournalTableService
    {
        private readonly ISysNumberSequenceService? _numbers;

        public async Task<IReadOnlyList<LedgerJournalOffsetAccountDto>> OffsetAccountsAsync(CancellationToken ct)
        {
            var area = _currentUser.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return [];
            var db = _unitOfWork.Context;
            var dimensions = db.Set<DimensionAttributeValueCombination>().AsNoTracking()
                .Where(x => x.DataAreaId == area && !x.IsDeleted);
            // Name the ledger offsets after their main account, not the account number again.
            var ledger = await (from dimension in dimensions.Where(x => x.MainAccount != 0)
                join account in db.Set<MainAccount>().AsNoTracking()
                    on dimension.MainAccount equals account.RecId into accounts
                from account in accounts.DefaultIfEmpty()
                select new LedgerJournalOffsetAccountDto(
                    dimension.RecId,
                    dimension.DisplayValue,
                    account != null && account.Name != "" ? account.Name : dimension.MainAccountValue,
                    0))
                .ToListAsync(ct);
            var banks = await (from dimension in dimensions
                join bank in db.Set<BankAccountTable>().AsNoTracking().Where(x => x.DataAreaId == area && !x.IsDeleted)
                    on dimension.DisplayValue equals bank.AccountId
                where dimension.MainAccount == 0 && dimension.MainAccountValue == ""
                select new LedgerJournalOffsetAccountDto(dimension.RecId, bank.AccountId, bank.Name, 5))
                .ToListAsync(ct);
            return ledger.Concat(banks).OrderBy(x => x.Code).ToList();
        }

        protected override async Task OnBeforeAddAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable entity, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(entity.OriginalJournalNum) || !string.IsNullOrWhiteSpace(entity.OriginalCompany))
                throw new InvalidOperationException("Reversal links are created only by the journal reversal process.");
            var area = string.IsNullOrWhiteSpace(entity.DataAreaId) ? _currentUser.GetDataAreaId() : entity.DataAreaId;
            if (string.IsNullOrWhiteSpace(area)) throw new InvalidOperationException("A company must be selected.");
            var setup = await _unitOfWork.Context.Set<LedgerJournalName>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.JournalName == entity.JournalName && !x.IsDeleted, ct)
                ?? throw new InvalidOperationException($"Journal name '{entity.JournalName}' is not configured.");
            entity.DataAreaId = area;
            entity.JournalType = setup.JournalType;
            entity.NumberSequenceTable = setup.NumberSequenceTable;
            entity.VoucherAllocatedAtPosting = setup.VoucherAllocatedAtPosting;
            entity.DefaultDimension = entity.DefaultDimension == 0 ? setup.DefaultDimension : entity.DefaultDimension;
            entity.CurrencyCode = string.IsNullOrWhiteSpace(entity.CurrencyCode) ? setup.CurrencyCode : entity.CurrencyCode;
            entity.OffsetLedgerDimension = setup.FixedOffsetAccount == NoYes.Yes || entity.OffsetLedgerDimension == 0
                ? setup.OffsetLedgerDimension : entity.OffsetLedgerDimension;
            entity.OffsetAccountType = setup.FixedOffsetAccount == NoYes.Yes || entity.OffsetLedgerDimension == setup.OffsetLedgerDimension
                ? setup.OffsetAccountType : entity.OffsetAccountType;
            entity.FixedOffsetAccount = setup.FixedOffsetAccount;
            entity.FixedExchRate = setup.FixedExchRate;
            entity.Posted = NoYes.No;
            entity.PostedDateTime = default;
            entity.JournalBalance = 0m;
            entity.JournalTotalDebit = 0m;
            entity.JournalTotalCredit = 0m;
            entity.JournalTotalDebitReportingCurrency = 0m;
            entity.JournalTotalCreditReportingCurrency = 0m;
            entity.NumOfLines = 0;
            entity.SystemBlocked = NoYes.No;
            if (string.IsNullOrWhiteSpace(entity.JournalNum))
                entity.JournalNum = _numbers == null
                    ? throw new InvalidOperationException("Journal batch number sequence service is unavailable.")
                    : await _numbers.GetNextSequenceAsync(nameof(LedgerJournalTable), area, ct);
        }

        public override async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable> UpdateAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable entity, CancellationToken ct = default)
        {
            await PrepareUpdateAsync(entity, ct);
            return await base.UpdateAsync(entity, ct);
        }

        private async Task PrepareUpdateAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable entity, CancellationToken ct)
        {
            var current = await EnsureEditableAsync(entity.RecId, ct);
            entity.DataAreaId = current.DataAreaId;
            entity.JournalNum = current.JournalNum;
            entity.JournalName = current.JournalName;
            entity.JournalType = current.JournalType;
            entity.NumberSequenceTable = current.NumberSequenceTable;
            entity.VoucherAllocatedAtPosting = current.VoucherAllocatedAtPosting;
            entity.Posted = current.Posted;
            entity.PostedDateTime = current.PostedDateTime;
            entity.JournalBalance = current.JournalBalance;
            entity.JournalTotalDebit = current.JournalTotalDebit;
            entity.JournalTotalCredit = current.JournalTotalCredit;
            entity.JournalTotalDebitReportingCurrency = current.JournalTotalDebitReportingCurrency;
            entity.JournalTotalCreditReportingCurrency = current.JournalTotalCreditReportingCurrency;
            entity.NumOfLines = current.NumOfLines;
            entity.SystemBlocked = current.SystemBlocked;
            entity.SystemBlockedReason = current.SystemBlockedReason;
            entity.OriginalJournalNum = current.OriginalJournalNum;
            entity.OriginalCompany = current.OriginalCompany;
        }

        public override async Task<IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>> UpdateRangeAsync(
            IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable> entities, CancellationToken ct = default)
        {
            var rows = entities.ToList();
            foreach (var row in rows) await PrepareUpdateAsync(row, ct);
            return await base.UpdateRangeAsync(rows, ct);
        }

        public override async Task RemoveRangeAsync(
            IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable> entities, CancellationToken ct = default)
        {
            var rows = entities.ToList();
            foreach (var row in rows) await EnsureRemovableAsync(row.RecId, ct);
            await base.RemoveRangeAsync(rows, ct);
        }

        public override async Task RemoveAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable entity, CancellationToken ct = default)
        {
            await EnsureRemovableAsync(entity.RecId, ct);
            await base.RemoveAsync(entity, ct);
        }

        private async Task EnsureRemovableAsync(long recId, CancellationToken ct)
        {
            var header = await EnsureEditableAsync(recId, ct);
            if (_unitOfWork.Context.ChangeTracker.Entries<LedgerJournalTrans>().Any(x =>
                    x.State == EntityState.Added && x.Entity.JournalNum == header.JournalNum &&
                    (x.Entity.DataAreaId == header.DataAreaId ||
                        (string.IsNullOrWhiteSpace(x.Entity.DataAreaId) && _currentUser.GetDataAreaId() == header.DataAreaId)))
                || await _unitOfWork.Context.Set<LedgerJournalTrans>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == header.DataAreaId && x.JournalNum == header.JournalNum, ct))
                throw new InvalidOperationException("Remove the journal lines before deleting its header.");
        }

        private async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable> EnsureEditableAsync(long recId, CancellationToken ct)
        {
            var current = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>()
                .AsNoTracking().SingleOrDefaultAsync(x => x.RecId == recId, ct)
                ?? throw new InvalidOperationException("Journal header was not found.");
            if (current.Posted == NoYes.Yes) throw new InvalidOperationException("Posted journals cannot be changed or deleted.");
            if (current.SystemBlocked == NoYes.Yes) throw new InvalidOperationException("The journal is system blocked.");
            return current;
        }
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
            if (header.VoucherAllocatedAtPosting != NoYes.Yes && lines.Any(x => string.IsNullOrWhiteSpace(x.Voucher)))
            {
                var name = await db.Set<LedgerJournalName>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area && x.JournalName == header.JournalName, ct);
                if (name != null && name.NumberSequenceTable != 0)
                {
                    var entered = lines.Where(x => !string.IsNullOrWhiteSpace(x.Voucher))
                        .OrderBy(x => x.LineNum).ThenBy(x => x.RecId).Select(x => x.Voucher).ToList();
                    string? shared = name.NewVoucher == LedgerJournalVoucherChange.None
                        && entered.Distinct(StringComparer.Ordinal).Count() == 1 ? entered[0] : null;
                    foreach (var line in lines.Where(x => string.IsNullOrWhiteSpace(x.Voucher)))
                    {
                        if (name.NewVoucher == LedgerJournalVoucherChange.None && shared != null) line.Voucher = shared;
                        else { line.Voucher = $"<pending:{Guid.NewGuid():N}>"; shared ??= line.Voucher; }
                    }
                }
            }
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
                    return (null, null);

                LedgerJournalSimulateResultDto result;
                try
                {
                    var posted = await new LedgerJournalPosting(db, _currentUser.GetCurrentUserId()).PostAsync(area, header, ct);
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
                await RollbackAndClearAsync(db);
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
                var result = await new LedgerJournalPosting(db, _currentUser.GetCurrentUserId()).PostAsync(area, header, ct);
                await _unitOfWork.CompleteAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return (result, null);
            }
            catch (SubledgerPostingException ex)
            {
                await RollbackAndClearAsync(db);
                return (null, ex.Message);
            }
            catch
            {
                await RollbackAndClearAsync(db);
                throw;
            }
        }

        private async Task RollbackAndClearAsync(DbContext db)
        {
            try
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            }
            finally
            {
                db.ChangeTracker.Clear();
            }
        }

        public LedgerJournalTableService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService numbers) : base(unitOfWork, currentUser)
        {
            _numbers = numbers;
        }

        public LedgerJournalTableService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser) { }

        public LedgerJournalTableService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
