namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesDeliveryAddressStatus { OrderNotFound, NotOpen, BadLineId, LineNotFound, ProcessedLine, Created }
public sealed record SalesDeliveryAddressResult(SalesDeliveryAddressStatus Status, long Id = 0,
    string Description = "", string Address = "");
