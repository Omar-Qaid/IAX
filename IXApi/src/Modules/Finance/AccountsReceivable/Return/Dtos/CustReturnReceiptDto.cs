namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

public sealed record CustReturnReceiptDto(string RequestId, DateTime? ReceiptDate, List<CustReturnReceiptLineDto> Lines);
