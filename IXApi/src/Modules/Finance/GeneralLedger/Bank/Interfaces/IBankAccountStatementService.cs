using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Bank
{
    public interface IBankAccountStatementService : IBaseService<BankAccountStatement>
    {
        Task<(BankAccountStatement? Statement, string? Error)> ReconcileAsync(
            string accountId, string statementNumber, DateTime statementDate, decimal endingBalance,
            IReadOnlyList<long> transactionIds, CancellationToken ct);
        Task<BankReconciliationDetail?> GetReconciliationAsync(long statementId, CancellationToken ct);
    }

    public sealed record BankReconciliationMovement(long RecId, DateTime TransDate, string Voucher, decimal Amount);
    public sealed record BankReconciliationDetail(long RecId, string AccountId, string StatementNumber,
        DateTime StatementDate, string CurrencyCode, decimal EndingBalance,
        IReadOnlyList<BankReconciliationMovement> Movements);
}
