namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesDiscountRecalculationStatus { NotFound, NotOpen, Recalculated }
public sealed record SalesDiscountRecalculationResult(SalesDiscountRecalculationStatus Status,
    int UpdatedLines = 0, decimal LineDiscount = 0, decimal MultiLineDiscount = 0, decimal TotalDiscountPercent = 0);
