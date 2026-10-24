namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

public sealed record CustReturnCreateDto(string InvoiceId, string? ReturnReasonCodeId, List<CustReturnLineDto> Lines);
