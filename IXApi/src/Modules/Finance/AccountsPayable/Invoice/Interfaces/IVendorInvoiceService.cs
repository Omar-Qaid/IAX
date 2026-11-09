using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendorInvoiceService : IBaseService<VendInvoiceJour>
{
    Task<PurchResult> PostAsync(long recId, VendorInvoicePostDto input, CancellationToken ct);
    Task<PurchResult> PostPrepaymentAsync(long orderRecId, string invoiceNumber, DateTime invoiceDate, decimal amount, CancellationToken ct);
    Task<PurchResult> ApplyPrepaymentAsync(long orderRecId, long prepaymentInvoiceRecId, long finalInvoiceRecId,
        DateTime accountingDate, decimal amount, CancellationToken ct);
    Task<PurchResult> ReversePrepaymentApplicationAsync(long orderRecId, long applicationRecId, DateTime accountingDate, CancellationToken ct);
}
