using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchReturnService : IBaseService<PurchTable>
{
    Task<PurchResult> CreateAsync(PurchReturnCreateDto input, CancellationToken ct);
    Task<PurchResult> ShipAsync(long returnRecId, PurchReturnShipDto input, CancellationToken ct);
    Task<PurchResult> CreditAsync(long returnRecId, VendCreditNoteDto input, CancellationToken ct);
}
