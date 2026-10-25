using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendorInvoiceService : IBaseService<VendInvoiceJour>
{
    Task<PurchResult> PostAsync(long recId, VendorInvoicePostDto input, CancellationToken ct);
}
