using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public interface ISalesInvoiceService : IBaseService<CustInvoiceJour>
{
    Task<SalesInvoiceResult> PostAsync(long salesRecId, SalesInvoicePostDto input, CancellationToken ct);
}
