namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record PurchOrderCreateDto(string OrderAccount, string? InvoiceAccount, string? CurrencyCode,
    DateTime? DeliveryDate, string InventSiteId, string InventLocationId, string? PurchName);

public sealed record PurchLineInputDto(string ItemId, decimal PurchQty, string? PurchUnit, decimal PurchPrice,
    decimal? PriceUnit, string? InventSiteId, string? InventLocationId, DateTime? DeliveryDate, string? Name);

public sealed record PurchResult(int StatusCode, object? Data = null, string? Message = null);
