namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IProductReceiptService
{
    Task<PurchResult> PostAsync(long recId, ProductReceiptPostDto input, CancellationToken ct);
}
