namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed record SalesOrderQuickCreateResult(SalesOrderListDto? Order, string? Error = null);
