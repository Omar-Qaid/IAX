using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

public interface IPackingSlipJournalService : IBaseService<CustPackingSlipJour>
{
    Task<PackingSlipJournalResult> ListAsync(long salesOrderRecId, CancellationToken ct);
    Task<PackingSlipJournalResult> GetAsync(long salesOrderRecId, long packingSlipRecId, CancellationToken ct);
    Task<PackingSlipJournalResult> PreviewPostingAsync(long salesOrderRecId, string quantityMode, CancellationToken ct);
    Task<PackingSlipJournalResult> PostAsync(long salesOrderRecId, PostPackingSlipRequest? request, CancellationToken ct);
}
