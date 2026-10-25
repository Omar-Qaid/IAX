using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

public interface ICustReturnService : IBaseService<SalesTable>
{
    Task<CustReturnResult> CreateAsync(CustReturnCreateDto input, CancellationToken ct);
    Task<CustReturnResult> ReceiveAsync(long returnRecId, CustReturnReceiptDto input, CancellationToken ct);
    Task<CustReturnResult> CreditAsync(long returnRecId, CustCreditNoteDto input, CancellationToken ct);
}
