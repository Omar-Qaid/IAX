using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public interface ISalesInvoiceService : IBaseService<CustInvoiceJour>
{
    Task<SalesInvoiceResult> ListCustomerLinesAsync(string accountNum, CancellationToken ct);
    Task<SalesInvoiceResult> ListJournalAsync(CancellationToken ct);
    Task<SalesInvoiceResult> PostAsync(long salesRecId, SalesInvoicePostDto input, CancellationToken ct);
}
