namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Return;

public sealed record CustReturnResult(int StatusCode, object? Data = null, string? Message = null);
