namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public interface ISalesInvoiceService
{
    Task<SalesInvoiceResult> PostAsync(long salesRecId, SalesInvoicePostDto input, CancellationToken ct);
}
