using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IProductReceiptService : IBaseService<VendPackingSlipJour>
{
    Task<PurchResult> PostAsync(long recId, ProductReceiptPostDto input, CancellationToken ct);
}
