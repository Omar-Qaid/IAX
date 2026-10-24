namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesLineWriteStatus { NotFound, Invalid, Success }
public sealed record SalesLineWriteResult(SalesLineWriteStatus Status, string? Error = null, object? Data = null);
