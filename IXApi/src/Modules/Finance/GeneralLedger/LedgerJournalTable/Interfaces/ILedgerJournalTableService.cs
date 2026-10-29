using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    public interface ILedgerJournalTableService : IBaseService<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>
    {
        Task<IReadOnlyList<string>?> CheckVoucherBalancesAsync(long recId, CancellationToken ct);
        Task<LedgerJournalSetupCheckDto?> CheckSetupAsync(long recId, CancellationToken ct);

        /// <summary>Posts the journal to the ledger. Returns (null, null) when the journal does not exist.</summary>
        Task<(LedgerJournalPostResultDto? Result, string? Error)> PostAsync(long recId, CancellationToken ct);
    }
}
