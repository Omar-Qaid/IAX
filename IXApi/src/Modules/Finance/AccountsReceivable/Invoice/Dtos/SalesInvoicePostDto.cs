namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed record SalesInvoicePostDto(DateTime? InvoiceDate = null, string? RequestId = null, List<SalesInvoiceLineDto>? Lines = null);
