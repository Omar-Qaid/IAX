namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

public sealed record CustPaymentResult(int StatusCode, object? Data = null, string? Message = null);
