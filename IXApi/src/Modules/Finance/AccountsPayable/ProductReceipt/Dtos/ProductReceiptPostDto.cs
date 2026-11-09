namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record PurchPostLineDto(long PurchLineRecId, decimal Qty, decimal? UnitPrice = null);
public sealed record ProductReceiptPostDto(string PackingSlipId, DateTime? DeliveryDate, List<PurchPostLineDto> Lines);
public sealed record ProductReceiptCorrectionDto(long ReceiptLineRecId, decimal Qty, DateTime AccountingDate);
public sealed record ProductReceiptCancelDto(DateTime AccountingDate);
