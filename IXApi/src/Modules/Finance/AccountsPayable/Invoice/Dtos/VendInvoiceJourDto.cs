using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendInvoiceJourDto : EntityDto<long>
{
    public string InvoiceId { get; set; } = string.Empty;
    public string InternalInvoiceId { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string PurchId { get; set; } = string.Empty;
    public string OrderAccount { get; set; } = string.Empty;
    public string InvoiceAccount { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string LedgerVoucher { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal SalesBalance { get; set; }
    public decimal InvoiceAmount { get; set; }
}
