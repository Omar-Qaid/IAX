namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed record SalesInvoiceResult(int StatusCode, object? Data = null, string? Message = null);
