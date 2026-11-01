using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Bank
{
    public class BankAccountStatementService : PostedLedgerReadService<BankAccountStatement>, IBankAccountStatementService
    {
        public async Task<BankReconciliationDetail?> GetReconciliationAsync(long statementId, CancellationToken ct)
        {
            var area = _currentUser.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return null;
            var statement = await _unitOfWork.Context.Set<BankAccountStatement>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == statementId, ct);
            if (statement == null) return null;
            var movements = await _unitOfWork.Context.Set<BankAccountTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.AccountId == statement.AccountId &&
                    x.AccountStatement == statement.AccountStatementNum && x.Reconciled == NoYes.Yes)
                .OrderBy(x => x.TransDate).ThenBy(x => x.RecId)
                .Select(x => new BankReconciliationMovement(x.RecId, x.TransDate, x.Voucher, x.BankTransAmountCur))
                .ToListAsync(ct);
            return new BankReconciliationDetail(statement.RecId, statement.AccountId, statement.AccountStatementNum,
                statement.AccountStatementDate, statement.CurrencyCode, statement.EndingBalance, movements);
        }

        public async Task<(BankAccountStatement? Statement, string? Error)> ReconcileAsync(
            string accountId, string statementNumber, DateTime statementDate, decimal endingBalance,
            IReadOnlyList<long> transactionIds, CancellationToken ct)
        {
            var area = _currentUser.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(statementNumber) || statementDate == default)
                return (null, "Bank account, statement number, and statement date are required.");
            if (accountId.Length > FieldLengths.AccountId || statementNumber.Length > FieldLengths.AccountStatementNum)
                return (null, "Bank account or statement number exceeds the allowed length.");
            if (transactionIds.Count == 0 || transactionIds.Distinct().Count() != transactionIds.Count)
                return (null, "Select distinct bank movements to reconcile.");
            var db = _unitOfWork.Context;
            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                await new SubledgerPostingSupport(db).LockAsync($"BankReconcile:{area}:{accountId}", ct);
                var bank = await db.Set<BankAccountTable>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area && x.AccountId == accountId, ct);
                if (bank == null) return await Reject("Bank account was not found in the selected company.");
                if (await db.Set<BankAccountStatement>().AnyAsync(x => x.DataAreaId == area &&
                    x.AccountId == accountId && x.AccountStatementNum == statementNumber, ct))
                    return await Reject("The statement number already exists for this bank account.");
                var previous = await db.Set<BankAccountStatement>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.AccountId == accountId && x.ReconcileDate != default)
                    .OrderByDescending(x => x.AccountStatementDate).FirstOrDefaultAsync(ct);
                if (previous != null && statementDate.Date <= previous.AccountStatementDate.Date)
                    return await Reject("The statement date must follow the last reconciled statement.");
                var movements = await db.Set<BankAccountTrans>()
                    .Where(x => x.DataAreaId == area && x.AccountId == accountId && transactionIds.Contains(x.RecId))
                    .ToListAsync(ct);
                if (movements.Count != transactionIds.Count || movements.Any(x => x.Reconciled == NoYes.Yes ||
                    x.Cancel == NoYes.Yes || x.TransDate.Date > statementDate.Date || x.BankTransCurrencyCode != bank.CurrencyCode))
                    return await Reject("Selected bank movements are missing, already reconciled, canceled, dated later, or in another currency.");
                var calculated = (previous?.EndingBalance ?? 0m) + movements.Sum(x => x.BankTransAmountCur);
                if (calculated != endingBalance)
                    return await Reject($"The statement ending balance differs from selected movements by {endingBalance - calculated}.");
                var statement = new BankAccountStatement
                {
                    DataAreaId = area, AccountId = accountId, AccountStatementNum = statementNumber,
                    AccountStatementDate = statementDate.Date, CurrencyCode = bank.CurrencyCode,
                    EndingBalance = endingBalance, ReconcileDate = DateTime.UtcNow
                };
                db.Add(statement);
                foreach (var movement in movements)
                {
                    movement.Reconciled = NoYes.Yes;
                    movement.AccountStatement = statementNumber;
                    movement.AccountStatementDate = statementDate.Date;
                    movement.ClearedDate = statementDate.Date;
                }
                await _unitOfWork.CompleteAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return (statement, null);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                throw;
            }

            async Task<(BankAccountStatement?, string?)> Reject(string error)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                return (null, error);
            }
        }
        public BankAccountStatementService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public BankAccountStatementService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
