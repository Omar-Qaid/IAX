namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record PurchOrderCreateDto(string OrderAccount, string? InvoiceAccount, string? CurrencyCode,
    DateTime? DeliveryDate, string InventSiteId, string InventLocationId, string? PurchName, DateTime? OrderDate = null);

/// <summary>
/// <paramref name="UsePriceAgreement"/> makes the line resolve a purchase trade agreement instead of
/// taking <paramref name="PurchPrice"/> verbatim, mirroring the sales line's flag of the same name.
/// It is opt-in, so a caller that supplies its own price keeps the existing behavior.
/// </summary>
public sealed record PurchLineInputDto(string ItemId, decimal PurchQty, string? PurchUnit, decimal PurchPrice,
    decimal? PriceUnit, string? InventSiteId, string? InventLocationId, DateTime? DeliveryDate, string? Name,
    bool UsePriceAgreement = false, string? InventBatchId = null, string? InventSerialId = null);

public sealed record PurchResult(int StatusCode, object? Data = null, string? Message = null);
