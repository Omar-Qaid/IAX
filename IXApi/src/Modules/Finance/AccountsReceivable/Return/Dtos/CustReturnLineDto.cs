namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

public sealed record CustReturnLineDto(long CustInvoiceTransRecId, decimal Qty, string DispositionCodeId);
