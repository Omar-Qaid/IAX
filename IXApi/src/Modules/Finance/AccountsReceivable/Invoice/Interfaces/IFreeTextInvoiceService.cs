namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public interface IFreeTextInvoiceService
{
    Task<IReadOnlyList<FreeTextInvoiceDto>> ListAsync(CancellationToken ct);
    Task<FreeTextInvoiceDto?> GetAsync(long recId, CancellationToken ct);
    Task<FreeTextInvoiceResult> CreateAsync(FreeTextInvoiceSaveDto input, CancellationToken ct);
    Task<FreeTextInvoiceResult> CreateCorrectionAsync(long originalRecId, FreeTextInvoiceSaveDto input, CancellationToken ct);
    Task<FreeTextInvoiceResult> UpdateAsync(long recId, FreeTextInvoiceSaveDto input, CancellationToken ct);
    Task<FreeTextInvoiceResult> DeleteAsync(long recId, CancellationToken ct);
    Task<FreeTextInvoiceResult> PostAsync(long recId, CancellationToken ct);
}
