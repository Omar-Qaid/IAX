namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

public interface ICustReturnService
{
    Task<CustReturnResult> CreateAsync(CustReturnCreateDto input, CancellationToken ct);
    Task<CustReturnResult> ReceiveAsync(long returnRecId, CustReturnReceiptDto input, CancellationToken ct);
    Task<CustReturnResult> CreditAsync(long returnRecId, CustCreditNoteDto input, CancellationToken ct);
}
