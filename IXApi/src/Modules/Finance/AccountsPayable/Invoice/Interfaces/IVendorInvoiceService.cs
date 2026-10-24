namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendorInvoiceService
{
    Task<PurchResult> PostAsync(long recId, VendorInvoicePostDto input, CancellationToken ct);
}
