namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record PurchReturnLineDto(long VendInvoiceTransRecId, decimal Qty);
public sealed record PurchReturnCreateDto(string VendInvoiceId, string? ReturnItemNum, List<PurchReturnLineDto> Lines);
public sealed record PurchReturnShipDto(string PackingSlipId, DateTime? ShipDate, List<PurchPostLineDto> Lines);
public sealed record VendCreditNoteDto(string InvoiceId, DateTime? InvoiceDate = null);
