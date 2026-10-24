namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesHeaderUpdateStatus { NotFound, Invalid, Saved }
public sealed record SalesHeaderUpdateResult(SalesHeaderUpdateStatus Status, string? Error = null);
