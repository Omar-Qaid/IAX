namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record PurchPostLineDto(long PurchLineRecId, decimal Qty, decimal? UnitPrice = null);
public sealed record ProductReceiptPostDto(string PackingSlipId, DateTime? DeliveryDate, List<PurchPostLineDto> Lines);
