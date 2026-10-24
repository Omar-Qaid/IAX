namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record VendorInvoicePostDto(string InvoiceId, DateTime? InvoiceDate, List<PurchPostLineDto> Lines);
