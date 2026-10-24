namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public record SalesOrderCopyRequest(string Mode, List<string> LineIds, decimal QuantityFactor = 1,
    bool InvertSign = false, bool RecalculatePrice = false, bool CopyPrecisely = true);
public enum SalesOrderCopyStatus { Success, DestinationNotFound, DestinationClosed, SourceLinesNotFound, InvalidUnit, InvalidRequest }
public sealed record SalesOrderCopyResult(SalesOrderCopyStatus Status, int CopiedLineCount = 0, string? Error = null);
